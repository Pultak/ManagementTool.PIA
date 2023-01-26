using System.Net;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;
using ManagementTool.Shared.Models.Presentation.Api.Requests;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController, Authorize]
public class ProjectsController : ControllerBase {
    public ProjectsController(IAuthService authService, IProjectsService projectsService,
        IUsersService usersService) {
        AuthService = authService;
        ProjectsService = projectsService;
        UsersService = usersService;
    }

    private IAuthService AuthService { get; }
    private IProjectsService ProjectsService { get; }
    private IUsersService UsersService { get; }

    /// <summary>
    /// Endpoint for getting all known available projects.
    /// This endpoint can be access by secretariat, departmentManager and Project manager.
    /// Project manager is only presented with projects he has rights to access
    /// </summary>
    /// <returns>All accessible projects based on the logged user</returns>
    [HttpGet]
    public IEnumerable<ProjectPL>? GetAllProjects() {
        Response.StatusCode = (int)HttpStatusCode.OK;
        if (AuthService.IsUserAuthorized(RoleType.Secretariat) ||
            AuthService.IsUserAuthorized(RoleType.DepartmentManager)) {
            var result = ProjectsService.GetProjects(null);
            return result;
        }

        if (AuthService.IsUserAuthorized(RoleType.ProjectManager)) {
            var projectIds = AuthService.GetAllProjectManagerProjectIds();
            if (!projectIds.Any()) {
                //is there anything in the returned array?
                return Enumerable.Empty<ProjectPL>();
            }

            var result = ProjectsService.GetProjects(projectIds);
            return result;
        }

        Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        return null;
    }


    /// <summary>
    /// This endpoint similarly to /api/Projects Get endpoint returns all projects based on logged user.
    /// The difference is that it also return a wrapper around the project that contains id and name of the assigned project manager.
    /// Secretariat and Department managers can access it without limitations.
    /// </summary>
    /// <returns>All accessible projects based on the logged user wrapped with project manager info</returns>
    [HttpGet("managers-wrapper")]
    public IEnumerable<ProjectInfoPayload>? GetAllProjectsWithManagers() {
        Response.StatusCode = (int)HttpStatusCode.OK;
        if (AuthService.IsUserAuthorized(RoleType.Secretariat) ||
            AuthService.IsUserAuthorized(RoleType.DepartmentManager)) {
            var result = ProjectsService.GetProjectsWithManagersInfo(null);
            return result;
        }

        if (AuthService.IsUserAuthorized(RoleType.ProjectManager)) {
            var projectIds = AuthService.GetAllProjectManagerProjectIds();
            if (!projectIds.Any()) {
                //is there anything in the returned array?
                return Enumerable.Empty<ProjectInfoPayload>();
            }

            var result = ProjectsService.GetProjectsWithManagersInfo(projectIds);
            return result;
        }

        Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        return null;
    }


    /// <summary>
    /// Endpoint for getting all users assigned to specified project
    /// This endpoint can be access by anyone
    /// </summary>
    /// <param name="projectId">valid id of the project</param>
    /// <returns>List of all users that are under desired project</returns>
    [HttpGet("{projectId:long}/users/assignations")]
    public IEnumerable<DataModelAssignmentPL<UserBasePL>>? GetAllUsersAssignationUnderProject(long projectId) {
        if (projectId < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }

        var resultUsers = UsersService.GetAllUsersAssignationsUnderProject(projectId);
        Response.StatusCode = (int)HttpStatusCode.OK;
        return resultUsers;
    }

    
    /// <summary>
    /// Endpoint for getting all users assigned to specified project
    /// This endpoint can be access by anyone
    /// </summary>
    /// <param name="projectId">valid id of the project</param>
    /// <returns>List of all users that are under desired project</returns>
    [HttpGet("{projectId:long}/users")]
    public IEnumerable<UserBasePL>? GetAllUsersUnderProject(long projectId) {
        if (projectId < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }

        var resultUsers = UsersService.GetAllUsersUnderProject(projectId);
        Response.StatusCode = (int)HttpStatusCode.OK;
        return resultUsers;
    }

    /// <summary>
    /// Endpoint for creation of new project.
    /// New projects can be created only by secretariat.
    /// The project object must be valid and without any possible data conflicts
    /// Otherwise and 4xx http response code will be returned
    /// </summary>
    /// <param name="project">new project object request</param>
    [HttpPost]
    public void CreateProject([FromBody] ProjectUpdateRequest project) {
        if (!AuthService.IsUserAuthorized(RoleType.Secretariat)) {
            //only secretariat can create projects
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        var creationStatus = ProjectsService.CreateProject(project);

        if (creationStatus != ProjectCreationResponse.Ok) {
            if (creationStatus == ProjectCreationResponse.NameTaken) {
                Response.StatusCode = (int)HttpStatusCode.Conflict;
            }
            else {
                Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            }

            return;
        }

        Response.StatusCode = (int)HttpStatusCode.OK;
    }

    /// <summary>
    /// Endpoint for updating of existing project.
    /// New projects can be updated without limits by secretariat and department manager.
    /// Project managers can edit them too, but they need to assigned to them first.
    /// The project object must be valid and without any possible data conflicts
    /// Otherwise and 4xx http response code will be returned
    /// </summary>
    /// <param name="projectRequest">project object that should be updated</param>
    [HttpPatch]
    public void UpdateProject([FromBody] ProjectUpdateRequest projectRequest) {
        if (!AuthService.IsAuthorizedToManageProjects(projectRequest.Project.Id)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        var updateOk = ProjectsService.UpdateProject(projectRequest);

        if (!updateOk) {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        }

        Response.StatusCode = (int)HttpStatusCode.OK;
    }

    /// <summary>
    /// Endpoint for deletion of existing project.
    /// Projects can be deleted only by secretariat.
    /// If the project is non existent the MethodNotAllowed status code is returned
    /// </summary>
    /// <param name="projectId">id of the project that should be deleted</param>
    [HttpDelete("{projectId:long}")]
    public void Delete(long projectId) {
        Response.StatusCode = (int)HttpStatusCode.OK;
        if (!AuthService.IsUserAuthorized(RoleType.Secretariat)) {
            //only secretariat can delete projects
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        var deleteOk = ProjectsService.DeleteProject(projectId);
        if (deleteOk) {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
        }
    }

    /// <summary>
    /// Endpoint for updating of assigned users for the project.
    /// Request should contain valid project id also with valid list of assigned users
    /// </summary>
    /// <param name="projectAssignRequest">request body with all needed data</param>
    [HttpPatch("users")]
    public void AssignUsersToProject([FromBody] ProjectAssignRequest projectAssignRequest) {
        if (!AuthService.IsUserAuthorized(RoleType.Secretariat)) {
            //only secretariat can assign users
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        var ok = ProjectsService.AssignUsersToProject(projectAssignRequest.AssignedUsers,
            projectAssignRequest.ProjectId);
        if (ok) {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        }
    }
}