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

    public ProjectsController(IProjectDataService projectService, IUserRoleDataService roleService){
        ProjectDataService = projectService;
        UserRoleDataService = roleService;
    }

    
    [HttpGet]
    public IEnumerable<Project> GetAllProjects() {

        //todo auth
        return ProjectDataService.GetAllProjects();
    }
    
    [HttpGet("{name}")]
    public Project? GetProjectByName(string name) {

        if (name == null) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }
        //todo other checks

        var project = ProjectDataService.GetProjectByName(name);
        if (project == null) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return null;
        }

        return project;
    }
    
    [HttpPut]
    public long CreateProject([FromBody] Project project) {

        if (project == null || string.IsNullOrEmpty(project.ProjectName)) {
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
            //todo better exception for client to capture
            Response.StatusCode = (int)HttpStatusCode.Conflict;
            return Response.StatusCode;
        }


        var projectId = ProjectDataService.AddProject(project);

        if (projectId < 0) {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return Response.StatusCode;
        }

        var newRole = new Role {
            Name = $"{project.ProjectName} Manager",
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
    public void UpdateProject([Microsoft.AspNetCore.Mvc.FromBody] Project project) {

        if (project == null || string.IsNullOrEmpty(project.ProjectName)){
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
    }
    
    [HttpDelete]
    public void Delete([Microsoft.AspNetCore.Mvc.FromBody] Project project) {
        if (project == null) {
            throw new BadHttpRequestException("Request body cant be empty!");
        }

        if (ProjectDataService.DeleteProject(project) &&
            UserRoleDataService.DeleteProjectRole(project.Id) &&
            ProjectDataService.DeleteProjectUserAssignments(project) &&
            ProjectDataService.DeleteAllProjectAssignments(project)) {
            return;
        }
        throw new BadHttpRequestException("The passed project is not valid for removal!");
    }



    private EProjectCreationResponse CheckProjectDataConflicts(Project project) {
        var allProjects = ProjectDataService.GetAllProjects();
        if (allProjects.Any(existingProject => existingProject.ProjectName.Equals(project.ProjectName))) {
            return EProjectCreationResponse.NameTaken;
        }
        return EProjectCreationResponse.Ok;
    }
}