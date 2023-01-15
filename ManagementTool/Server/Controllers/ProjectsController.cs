using System.Net;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers;

[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
[ApiController]
public class ProjectsController : ControllerBase {
    public IProjectDataService ProjectDataService { get; }
    public IUserRoleDataService UserRoleDataService { get; }
    public IUserDataService UserDataService { get; }

    public ProjectsController(IProjectDataService projectService, IUserRoleDataService roleService, IUserDataService userService){
        ProjectDataService = projectService;
        UserRoleDataService = roleService;
        UserDataService = userService;
    }

    
    [HttpGet]
    public IEnumerable<Project>? GetAllProjects() {
        var roles = LoginController.GetLoggedUserRoles(HttpContext.Session);
        if (roles == null) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        if (LoginController.IsUserAuthorized(ERoleType.Secretariat, roles) ||
            LoginController.IsUserAuthorized(ERoleType.DepartmentManager, roles)) {

            return ProjectDataService.GetAllProjects();
        }
        else if (LoginController.IsUserAuthorized(ERoleType.ProjectManager, roles)) {

            var managerRoles = LoginController.GetAllProjectManagerRoles(roles);
            var projectIds = managerRoles.Select(x => x.ProjectId).OfType<long>().ToList();
            return ProjectDataService.GetProjectsByIds(projectIds);
        }


        Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        return null;
    }
    
    [HttpGet("{name}")]
    public Project? GetProjectByName(string name) {
        if (LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        
        if (string.IsNullOrEmpty(name)) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }

        var project = ProjectDataService.GetProjectByName(name);
        if (project == null) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return null;
        }

        return project;
    }

    [HttpGet("{projectId:long}/users")]
    public IEnumerable<UserBase>? GetAllUsersUnderProject(long projectId) {
        if (LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        if (projectId < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }

        var resultUsers = UserDataService.GetAllUsersUnderProject(projectId);
        return resultUsers;
    }


    [HttpPut]
    public long CreateProject([FromBody] Project project) {
        
        if (LoginController.IsUserAuthorized(ERoleType.Secretariat, HttpContext.Session)) {
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


        var projectId = ProjectDataService.AddProject(project);

        if (projectId < 0) {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return Response.StatusCode;
        }

        var newRole = new Role {
            Name = $"{project.ProjectName} Manažer",
            Type = ERoleType.ProjectManager,
            ProjectId = projectId
        };
        var roleId = UserRoleDataService.AddRole(newRole);

        if (roleId < 0) {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return Response.StatusCode;
        }
        return projectId;
    }


    [HttpPatch("update")]
    public void UpdateProject([FromBody] Project project) {
        if (IsAuthorizedToManageProjects(project.Id)) {
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
        
        var updateOk = ProjectDataService.UpdateProject(project);
        
        if (!updateOk) {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }

        UserRoleDataService.UpdateProjectRoleName(project.Id, project.ProjectName + " Manažer");
    }
    
    [HttpDelete]
    public void Delete([FromBody] Project project) {
        if (IsAuthorizedToManageProjects(project.Id)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }
        if (project.Id < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        if (ProjectDataService.DeleteProject(project) &&
            UserRoleDataService.DeleteProjectRole(project.Id) &&
            ProjectDataService.DeleteProjectUserAssignments(project) &&
            ProjectDataService.DeleteAllProjectAssignments(project)) {
            return;
        }

        Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }



    private EProjectCreationResponse CheckProjectDataConflicts(Project project) {
        var allProjects = ProjectDataService.GetAllProjects();
        if (allProjects.Any(existingProject => existingProject.ProjectName.Equals(project.ProjectName))) {
            return EProjectCreationResponse.NameTaken;
        }
        return EProjectCreationResponse.Ok;
    }

    

    private bool IsAuthorizedToManageProjects(long projectId) {
        var userRoles = LoginController.GetLoggedUserRoles(HttpContext.Session);
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
        return managerRoles.Any(x => x.ProjectId == relevantProjectId);
    }

}