using System.Net;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers;

[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
[ApiController]
public class ProjectsController : ControllerBase {
    public IProjectRepository ProjectRepository { get; }
    public IUserRoleRepository UserRoleRepository { get; }
    public IUserRepository UserRepository { get; }

    public ProjectsController(IProjectRepository projectService, IUserRoleRepository roleService, IUserRepository userService){
        ProjectRepository = projectService;
        UserRoleRepository = roleService;
        UserRepository = userService;
    }

    
    [HttpGet]
    public Project[]? GetAllProjects() {
        var roles = LoginController.GetLoggedUserRoles(HttpContext.Session);
        if (roles == null) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        Response.StatusCode = (int)HttpStatusCode.OK;
        if (LoginController.IsUserAuthorized(ERoleType.Secretariat, roles) ||
            LoginController.IsUserAuthorized(ERoleType.DepartmentManager, roles)) {

            return ProjectRepository.GetAllProjects().ToArray();
        }
        if (LoginController.IsUserAuthorized(ERoleType.ProjectManager, roles)) {

            var managerRoles = LoginController.GetAllProjectManagerRoles(roles);
            var projectIds = managerRoles.Select(x => x.ProjectId).OfType<long>().ToList();
            return ProjectRepository.GetProjectsByIds(projectIds).ToArray();
        }


        Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        return null;
    }
    
    [HttpGet("{name}")]
    public Project? GetProjectByName(string name) {
        if (!LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        
        if (string.IsNullOrEmpty(name)) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }

        var project = ProjectRepository.GetProjectByName(name);
        if (project == null) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return null;
        }

        Response.StatusCode = (int)HttpStatusCode.OK;
        return project;
    }

    [HttpGet("{projectId:long}/users")]
    public IEnumerable<UserBase>? GetAllUsersUnderProject(long projectId) {
        if (!LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        if (projectId < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }

        var resultUsers = UserRepository.GetAllUsersUnderProject(projectId);
        Response.StatusCode = (int)HttpStatusCode.OK;
        return resultUsers;
    }


    [HttpPut]
    public long CreateProject([FromBody] Project project) {
        
        if (!LoginController.IsUserAuthorized(ERoleType.Secretariat, HttpContext.Session)) {
            //only secretariat can create projects
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return -1;
        }

        if (string.IsNullOrEmpty(project.ProjectName)) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Response.StatusCode;
        }

        var valResult = ProjectUtils.ValidateNewProject(project);
        if (valResult != EProjectCreationResponse.Ok) {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            return Response.StatusCode;
        }

        valResult = CheckProjectDataConflicts(project);
        if (valResult != EProjectCreationResponse.Ok) {
            Response.StatusCode = (int)HttpStatusCode.Conflict;
            return Response.StatusCode;
        }


        var projectId = ProjectRepository.AddProject(project);

        if (projectId < 0) {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return Response.StatusCode;
        }

        var newRole = new Role {
            Name = $"{project.ProjectName} Manažer",
            Type = ERoleType.ProjectManager,
            ProjectId = projectId
        };
        var roleId = UserRoleRepository.AddRole(newRole);

        if (roleId < 0) {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return Response.StatusCode;
        }
        Response.StatusCode = (int)HttpStatusCode.OK;
        return projectId;
    }


    [HttpPatch("update")]
    public void UpdateProject([FromBody] Project project) {
        if (!IsAuthorizedToManageProjects(project.Id, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        if (string.IsNullOrEmpty(project.ProjectName)){
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        var valResult = ProjectUtils.ValidateNewProject(project);
        if (valResult != EProjectCreationResponse.Ok) {
            Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
            return;
        }
        
        var updateOk = ProjectRepository.UpdateProject(project);
        
        if (!updateOk) {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }

        UserRoleRepository.UpdateProjectRoleName(project.Id, project.ProjectName + " Manažer");
        Response.StatusCode = (int)HttpStatusCode.OK;
    }
    
    [HttpDelete]
    public void Delete([FromBody] Project project) {
        Response.StatusCode = (int)HttpStatusCode.OK;
        if (!IsAuthorizedToManageProjects(project.Id, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }
        if (project.Id < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        if (ProjectRepository.DeleteProject(project) &&
            UserRoleRepository.DeleteProjectRole(project.Id) &&
            ProjectRepository.DeleteProjectUserAssignments(project) &&
            ProjectRepository.DeleteAllProjectAssignments(project)) {
            return;
        }

        Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }



    private EProjectCreationResponse CheckProjectDataConflicts(Project project) {
        var allProjects = ProjectRepository.GetAllProjects();
        if (allProjects.Any(existingProject => existingProject.ProjectName.Equals(project.ProjectName))) {
            return EProjectCreationResponse.NameTaken;
        }
        return EProjectCreationResponse.Ok;
    }

    

    public static bool IsAuthorizedToManageProjects(long projectId, ISession session) {
        var userRoles = LoginController.GetLoggedUserRoles(session);
        if (userRoles == null) {
            return false;
        }
        if (LoginController.IsUserAuthorized(ERoleType.DepartmentManager, userRoles) ||
            LoginController.IsUserAuthorized(ERoleType.Secretariat, userRoles)) {
            //all ok, department manager and secretariat can do everything with projects
            return true;
        }

        if (!LoginController.IsUserAuthorized(ERoleType.ProjectManager, userRoles)) {
            return false;
        }

        var managerRoles = LoginController.GetAllProjectManagerRoles(userRoles);

        //can this manager manage assignments for this project?
        return managerRoles.Any(x => x.ProjectId == projectId);
    }

}