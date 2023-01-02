using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectsController : ControllerBase {

    public static readonly DateTime RefDateTime = new(2010, 1, 1);

    public IProjectDataService ProjectDataService { get; }
    public IUserRoleDataService UserRoleDataService { get; }

    public ProjectsController(IProjectDataService projectService, IUserRoleDataService roleService){
        ProjectDataService = projectService;
        UserRoleDataService = roleService;
    }


    // GET: api/<ProjectsController>
    [HttpGet]
    public IEnumerable<Project> GetAllProjects() {

        //todo auth
        return ProjectDataService.GetAllProjects();
    }

    // GET api/<ProjectsController>/5
    [HttpGet("{name}")]
    public IActionResult GetProjectByName(string name) {

        if (name == null) {
            return BadRequest();
        }
        //todo other checks

        var project = ProjectDataService.GetProjectByName(name);
        if (project == null) {
            return NotFound();
        }

        return Ok(project);
    }

    // POST api/<ProjectsController>
    [HttpPost]
    public IActionResult CreateProject([FromBody] Project project) {

        if (project == null || string.IsNullOrEmpty(project.ProjectName)){
            return BadRequest(EProjectCreationResponse.EmptyProject);
        }

        var valResult = ValidateNewProject(project);
        if (valResult != EProjectCreationResponse.Ok) {
            return UnprocessableEntity(valResult);
        }

        valResult = CheckProjectDataConflicts(project);
        if (valResult != EProjectCreationResponse.Ok) {
            return Conflict(valResult);
        }


        var projectId = ProjectDataService.AddProject(project);

        if (projectId < 0) {
            //todo saving failed!
            return StatusCode(500);
        }

        var newRole = new Role(-1, $"{project.ProjectName}Manager", ERoleType.ProjectManager, projectId);
        var roleId = UserRoleDataService.AddRole(newRole);

        if (roleId < 0) {
            //todo saving failed!
            return StatusCode(500);
        }
        return Ok(projectId);
    }

    // DELETE api/<ProjectsController>/5
    [HttpDelete]
    public void Delete([FromBody] Project project) {
        //todo delete project, role + all role assignments
    }




    private EProjectCreationResponse ValidateNewProject(Project project) {

        if (project.ProjectName.Length is < 2 or > 256) {
            return EProjectCreationResponse.InvalidName;
        }

        var timeDiff = DateTime.Compare(RefDateTime, project.FromDate);
        if (timeDiff < 0) {
            //earlier from date than allowed!
            return EProjectCreationResponse.InvalidFromDate;
        }

        if (project.ToDate != null) {
            timeDiff = DateTime.Compare(project.FromDate, project.ToDate.Value);
            if (timeDiff <= 0) {
                //to date is earlier or from the same time
                return EProjectCreationResponse.InvalidToDate;
            }
        }

        if (project.Description.Length > 256) {
            return EProjectCreationResponse.InvalidDescription;
        }

        return EProjectCreationResponse.Ok;
    }


    private EProjectCreationResponse CheckProjectDataConflicts(Project project) {
        var allProjects = ProjectDataService.GetAllProjects();
        if (allProjects.Any(existingProject => existingProject.ProjectName.Equals(project.ProjectName))) {
            return EProjectCreationResponse.NameTaken;
        }
        return EProjectCreationResponse.Ok;
    }
}