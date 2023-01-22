using System.Net;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Requests;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Mvc;

namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectsController : ControllerBase {

    private IAuthService AuthService { get; }
    private IProjectsService ProjectsService { get; }
    private IUsersService UsersService { get; }

    public ProjectsController(IAuthService authService, IProjectsService projectsService, 
        IUsersService usersService) {
        AuthService = authService;
        ProjectsService = projectsService;
        UsersService = usersService;
    }

    
    [HttpGet]
    public IEnumerable<ProjectPL>? GetAllProjects() {
        Response.StatusCode = (int)HttpStatusCode.OK;
        if (AuthService.IsUserAuthorized(ERoleType.Secretariat) ||
            AuthService.IsUserAuthorized(ERoleType.DepartmentManager)) {
            var result = ProjectsService.GetProjects(null);
            return result;
        }
        if (AuthService.IsUserAuthorized(ERoleType.ProjectManager)) {
            var projectIds = AuthService.GetAllProjectManagerProjectIds();
            if (!projectIds.Any()) {
                //is there anything in the returned array?
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return Enumerable.Empty<ProjectPL>();
            }
            var result = ProjectsService.GetProjects(projectIds);
            return result;
        }

        Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        return null;
    }
    
    [HttpGet("{projectId:long}/users")]
    public IEnumerable<DataModelAssignmentPL<UserBasePL>>? GetAllUsersUnderProject(long projectId) {
        if (!AuthService.IsUserAuthorized(null)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        if (projectId < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }

        var resultUsers = UsersService.GetAllUsersUnderProject(projectId);
        Response.StatusCode = (int)HttpStatusCode.OK;
        return resultUsers;
    }

    
    [HttpGet("{idProject:long}/users")]
    public IEnumerable<DataModelAssignmentPL<UserBasePL>>? GetAllUsersForProject(long idProject) {
        if (!AuthService.IsAuthorizedToManageProjects(idProject)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        if (idProject < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }
        //todo move to service
        var result = UsersService.GetAllUsersUnderProject(idProject);
        Response.StatusCode = (int)HttpStatusCode.OK;
        return result;
    }

    //todo changed 
    [HttpPost]
    public void CreateProject([FromBody] ProjectPL project) {
        if (!AuthService.IsUserAuthorized(ERoleType.Secretariat)) {
            //only secretariat can create projects
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        var creationStatus = ProjectsService.CreateProject(project);

        if (creationStatus != EProjectCreationResponse.Ok) {
            if (creationStatus == EProjectCreationResponse.NameTaken) {
                Response.StatusCode = (int)HttpStatusCode.Conflict;
            }
            else {
                Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            }
            return;
        }
        
        Response.StatusCode = (int)HttpStatusCode.OK;
    }


    [HttpPatch("update")]
    public void UpdateProject([FromBody] ProjectPL project) {
        if (!AuthService.IsAuthorizedToManageProjects(project.Id)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        var updateOk = ProjectsService.UpdateProject(project);

        if (!updateOk) {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        }
        Response.StatusCode = (int)HttpStatusCode.OK;
    }
    
    //todo needs to be implemented
    [HttpDelete("{projectId:long}")]
    public void Delete(long projectId) {
        Response.StatusCode = (int)HttpStatusCode.OK;
        if (!AuthService.IsUserAuthorized(ERoleType.Secretariat)) {
            //only secretariat can delete projects
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        var deleteOk = ProjectsService.DeleteProject(projectId);
        if (deleteOk) {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        }
    }

    //todo moved and changed uri
    [HttpPatch("users")]
    public void AssignUsersToProject([FromBody]ProjectAssignRequest projectAssignRequest) {
        if (!AuthService.IsUserAuthorized(ERoleType.Secretariat)) {
            //only secretariat can assign users
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        var ok = ProjectsService.AssignUsersToProject(projectAssignRequest.AssignedUsers, projectAssignRequest.ProjectId);
        if (ok) {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        }
    }
}