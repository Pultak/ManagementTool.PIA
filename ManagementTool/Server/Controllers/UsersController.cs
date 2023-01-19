using System.Net;
using System.Security.Cryptography;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.ApiModels;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase {
    
    public IUserRepository UserRepository { get; }
    public IProjectRepository ProjectRepository { get; }
    public IUserRoleRepository RoleRepository { get; }

    public UsersController(IUserRepository userService, IProjectRepository projectService, IUserRoleRepository roleRepository) {
        UserRepository = userService;
        ProjectRepository = projectService;
        RoleRepository = roleRepository;
    }
    
    [HttpGet]
    public IEnumerable<UserBase>? GetAllUsers() {
        if (!LoginController.IsUserAuthorized(ERoleType.Secretariat, HttpContext.Session) &&
            !LoginController.IsUserAuthorized(ERoleType.ProjectManager, HttpContext.Session) &&
            !LoginController.IsUserAuthorized(ERoleType.DepartmentManager, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        Response.StatusCode = (int)HttpStatusCode.OK;
        return UserRepository.GetAllUsers();
    }


    [HttpGet("projectUsers/{idProject:long}")]
    public IEnumerable<DataModelAssignment<UserBase>>? GetAllUsersForProject(long idProject) {
        if (!ProjectsController.IsAuthorizedToManageProjects(idProject, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        if (idProject < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }

        var result = UserRepository.GetAllUsersAssignedToProject(idProject);
        Response.StatusCode = (int)HttpStatusCode.OK;
        return result;
    }
    /*todo remove
    [HttpGet("{id:long}")]
    public UserBase? GetUserById(long id) {
        if (!LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        
        if (id < 0) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }

        var user = UserRepository.GetUserById(id);

        if (user == null) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return null;
        }
        
        return user;
    }*/

    [HttpPut]
    public long CreateUser([FromBody] UserUpdatePayload<User> userPayload) {
        if (!LoginController.IsUserAuthorized(ERoleType.Secretariat, HttpContext.Session)) {
            //only secretariat can 
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return -1;
        }

        var valResult = UserUtils.ValidateUser(userPayload.UpdatedUser);
        if (valResult != EUserCreationResponse.Ok || !UserUtils.IsValidPassword(userPayload.UpdatedUser.Pwd)) {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            return -1;
        }
        
        valResult = CheckUserDataConflicts(userPayload.UpdatedUser);
        if (valResult != EUserCreationResponse.Ok) {
            Response.StatusCode = (int)HttpStatusCode.Conflict;
            return -1;
        }

        var salt = GenerateSalt(); 

        userPayload.UpdatedUser.Pwd = LoginController.HashPwd(userPayload.UpdatedUser.Pwd, salt);
        userPayload.UpdatedUser.Salt = Convert.ToBase64String(salt);
        

        var userId = UserRepository.AddUser(userPayload.UpdatedUser);
        userPayload.UpdatedUser.Id = userId;

        if (userId < 0) {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return -1;
        }

        UpdateUserRoleAssignments(userPayload.AssignedRoles, userId);
        UpdateUserSuperiorAssignments(userPayload.Superiors, userPayload.UpdatedUser);

        Response.StatusCode = (int)HttpStatusCode.OK;
        return userId;
    }


    [HttpPatch("update")]
    public void UpdateUser([FromBody] UserUpdatePayload<UserBase> userPayload) {
        if (!LoginController.IsUserAuthorized(ERoleType.Secretariat, HttpContext.Session)) {
            //only secretariat can update users
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }
        var valResult = UserUtils.ValidateUser(userPayload.UpdatedUser);
        if (valResult == EUserCreationResponse.Ok) {
            
            var dbUser = new User(userPayload.UpdatedUser);
            UserRepository.UpdateUser(dbUser);
            UpdateUserRoleAssignments(userPayload.AssignedRoles, userPayload.UpdatedUser.Id);
            UpdateUserSuperiorAssignments(userPayload.Superiors, userPayload.UpdatedUser);
            Response.StatusCode = (int)HttpStatusCode.OK;
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
    }

    [HttpDelete("{id}")]
    public void DeleteUser(long id) {
        if (!LoginController.IsUserAuthorized(ERoleType.Secretariat, HttpContext.Session)) {
            //only secretariat can delete users
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }
        if (id < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        var user = UserRepository.GetUserById(id);

        if (user == null) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }
        var ok = UserRepository.DeleteUser(user);
        Response.StatusCode = (int)HttpStatusCode.OK;

        if (!ok) {
            Response.StatusCode = (int)HttpStatusCode.NoContent;
        }
    }


    
    [HttpPost("assignUser")]
    public void AssignUsersToProject([FromBody]ProjectAssignPayload projectAssignPayload) {
        
        if (!LoginController.IsUserAuthorized(ERoleType.Secretariat, HttpContext.Session)) {
            //only secretariat can assign users
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }
        if (projectAssignPayload.ProjectId < 0) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        var project = ProjectRepository.GetProjectById(projectAssignPayload.ProjectId);
        if (project == null) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }
        
        var ok = UpdateUserProjectAssignments(projectAssignPayload.AssignedUsers, project);
        Response.StatusCode = (int)HttpStatusCode.OK;
        if (!ok) {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }
    

    private EUserCreationResponse CheckUserDataConflicts(User user) {
        var allUsers = UserRepository.GetAllUsers();
        if (allUsers.Any(existingUser => existingUser.Username.Equals(user.Username))) {
            return EUserCreationResponse.UsernameTaken;
        }
        return EUserCreationResponse.Ok;
    }
    
    private bool UpdateUserProjectAssignments(IReadOnlyCollection<UserBase> assignedUsers, Project project) {
        var dbProjectAssignees = UserRepository.GetAllUsersAssignedToProject(project.Id).ToArray();
        
        List<long> unassignList = new();
        List<long> assignList = new();
        foreach (var projectAssignee in dbProjectAssignees) {
            if (projectAssignee.IsAssigned) {
                if (assignedUsers.All(role => role.Id != projectAssignee.DataModel.Id)) {
                    //we need to remove reference
                    unassignList.Add(projectAssignee.DataModel.Id);
                }
                //assigned and should be assigned => nothing changed
            }
            else {
                if (assignedUsers.Any(role => role.Id == projectAssignee.DataModel.Id)) {
                    //we need to add reference
                    assignList.Add(projectAssignee.DataModel.Id);
                }
            }
        }
        if (assignList.Count > 0) {
            UserRepository.AssignUsersToProject(assignList, project);
        }

        if (unassignList.Count > 0) {
            UserRepository.UnassignUsersFromProject(unassignList, project);
        }

        return true;
    }
    
    
    private void UpdateUserSuperiorAssignments(IReadOnlyCollection<UserBase> newAssignedSuperiors, UserBase user) {
        var dbAssignedSuperiors = UserRepository.GetAllUserSuperiorsIds(user.Id).ToArray();

        //select all superior references that should be removed from db
        //if not present in newList then remove from db
        var unassignList = dbAssignedSuperiors.Where(x => newAssignedSuperiors.All(newAssign => newAssign.Id != x)).ToList();
        
        //select all superior references that should be added to the db
        //if not present in dbList then add to db
        var assignList = newAssignedSuperiors.Where(x => !dbAssignedSuperiors.Contains(x.Id)).Select(x=> x.Id).ToList();
        
        if (assignList.Count > 0) {
            UserRepository.AssignSuperiorsToUser(assignList, user);
        }

        if (unassignList.Count > 0) {
            UserRepository.UnassignSuperiorsFromUser(unassignList, user);
        }
    }


    private void UpdateUserRoleAssignments(List<DataModelAssignment<Role>> roleAssignments, long userId) {
        var userRoles = RoleRepository.GetUserRolesByUserId(userId).ToArray();
        
        List<Role> unassignList = new();
        List<Role> assignList = new();
        foreach (var roleAssignment in roleAssignments) {
            if (roleAssignment.IsAssigned) {
                if (userRoles.All(role => role.Id != roleAssignment.DataModel.Id)) {
                    //we need to add reference
                    assignList.Add(roleAssignment.DataModel);
                }
                //nothing changed
            }
            else {
                if (userRoles.Any(role => role.Id == roleAssignment.DataModel.Id)) {
                    //we need to remove reference
                    unassignList.Add(roleAssignment.DataModel);
                }
            }
        }

        if (assignList.Count > 0) {
            RoleRepository.AssignRolesToUser(assignList, userId);
        }

        if (unassignList.Count > 0) {
            RoleRepository.UnassignRolesFromUser(unassignList, userId);
        }
    }


    public static byte[] GenerateSalt() {
        // Generate a 128-bit salt using a sequence of
        // cryptographically strong random bytes.
        return RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes
    }
}