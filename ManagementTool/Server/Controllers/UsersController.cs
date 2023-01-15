using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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


    public IUserDataService UserDataService { get; }
    public IProjectDataService ProjectDataService { get; }
    public IUserRoleDataService RoleDataService { get; }

    public UsersController(IUserDataService userService, IProjectDataService projectService, IUserRoleDataService roleDataService) {
        UserDataService = userService;
        ProjectDataService = projectService;
        RoleDataService = roleDataService;
    }
    
    [HttpGet]
    public IEnumerable<UserBase>? GetAllUsers() {
        if (!LoginController.IsUserAuthorized(ERoleType.Secretariat, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        return UserDataService.GetAllUsers();
    }


    [HttpGet("projectUsers/{idProject:long}")]
    public IEnumerable<DataModelAssignment<UserBase>>? GetAllUsersForProject(long idProject) {
        if (!LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        if (idProject < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }

        var result = UserDataService.GetAllUsersAssignedToProject(idProject);
        return result;
    }

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

        var user = UserDataService.GetUserById(id);

        if (user == null) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return null;
        }
        
        return user;
    }

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

        // Generate a 128-bit salt using a sequence of
        // cryptographically strong random bytes.
        var salt = RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes

        userPayload.UpdatedUser.Pwd = LoginController.HashPwd(userPayload.UpdatedUser.Pwd, salt);
        userPayload.UpdatedUser.Salt = Convert.ToBase64String(salt);
        

        var userId = UserDataService.AddUser(userPayload.UpdatedUser);

        if (userId < 0) {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return -1;
        }

        UpdateUserRoleAssignments(userPayload.AssignedRoles, userId);

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
            UserDataService.UpdateUser(dbUser);
            UpdateUserRoleAssignments(userPayload.AssignedRoles, userPayload.UpdatedUser.Id);
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
    }

    [HttpDelete]
    public void Delete([FromBody] UserBase user) {
        if (!LoginController.IsUserAuthorized(ERoleType.Secretariat, HttpContext.Session)) {
            //only secretariat can delete users
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }
        var dbUser = new User(user);
        var ok = UserDataService.DeleteUser(dbUser);

        if (!ok) {
            Response.StatusCode = (int)HttpStatusCode.NoContent;
        }
    }

    [HttpDelete("{id}")]
    public void Delete(long id) {
        if (!LoginController.IsUserAuthorized(ERoleType.Secretariat, HttpContext.Session)) {
            //only secretariat can delete users
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }
        if (id < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        var user = UserDataService.GetUserById(id);

        if (user == null) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }
        var ok = UserDataService.DeleteUser(user);

        if (!ok) {
            Response.StatusCode = (int)HttpStatusCode.NoContent;
        }
    }


    
    [HttpPut("assignUser/{idUser:long}/{idProject:long}")]
    public void AssignUserToProject(long idUser, long idProject) {
        
        if (!LoginController.IsUserAuthorized(ERoleType.Secretariat, HttpContext.Session)) {
            //only secretariat can assign users
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }
        if (idUser < 0 && idProject < 0) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        var user = UserDataService.GetUserById(idUser);
        if (user == null) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }

        var project = ProjectDataService.GetProjectById(idProject);
        if (project == null) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }
        
        var ok = UserDataService.AssignUserToProject(user, project);
        if (!ok) {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }
    

    private EUserCreationResponse CheckUserDataConflicts(User user) {
        var allUsers = UserDataService.GetAllUsers();
        if (allUsers.Any(existingUser => existingUser.Username.Equals(user.Username))) {
            return EUserCreationResponse.UsernameTaken;
        }
        return EUserCreationResponse.Ok;
    }

    
    private bool UpdateUserRoleAssignments(List<DataModelAssignment<Role>> roleAssignments, long userId) {
        var userRoles = RoleDataService.GetUserRolesByUserId(userId).ToArray();
        
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
            RoleDataService.AssignRolesToUser(assignList, userId);
        }

        if (unassignList.Count > 0) {
            RoleDataService.UnassignRolesFromUser(unassignList, userId);
        }

        return true;
    }
}