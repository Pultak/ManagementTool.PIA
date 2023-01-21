﻿using System.Net;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Roles;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Api.Payloads;
using ManagementTool.Shared.Models.Api.Requests;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectsController : ControllerBase {

    private IAuthService AuthService { get; }
    private IProjectsService ProjectsService { get; }
    private IUsersService UsersService { get; }
    private IRolesService RolesService { get; }

    public ProjectsController(IAuthService authService, IProjectsService projectsService, 
        IUsersService usersService, IRolesService rolesService) {
        AuthService = authService;
        ProjectsService = projectsService;
        UsersService = usersService;
        RolesService = rolesService;
    }

    
    [HttpGet]
    public IEnumerable<Project>? GetAllProjects() {
        var roles = AuthService.GetLoggedUserRoles();
        if (roles == null) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        Response.StatusCode = (int)HttpStatusCode.OK;
        if (AuthService.IsUserAuthorized(ERoleType.Secretariat, roles) ||
            AuthService.IsUserAuthorized(ERoleType.DepartmentManager, roles)) {
            var result = ProjectsService.GetProjects(null);
            return result;
        }
        if (AuthService.IsUserAuthorized(ERoleType.ProjectManager, roles)) {
            var projectIds = AuthService.GetAllProjectManagerProjectIds();
            if (!projectIds.Any()) {
                //is there anything in the returned array?
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return Enumerable.Empty<Project>();
            }
            var result = ProjectsService.GetProjects(projectIds);
            return result;
        }

        Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        return null;
    }
    
    [HttpGet("{projectId:long}/users")]
    public IEnumerable<DataModelAssignment<UserBase>>? GetAllUsersUnderProject(long projectId) {
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
    public IEnumerable<DataModelAssignment<UserBase>>? GetAllUsersForProject(long idProject) {
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
    public void CreateProject([FromBody] Project project) {
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
    public void UpdateProject([FromBody] Project project) {
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
    [HttpDelete("{projectId:long")]
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