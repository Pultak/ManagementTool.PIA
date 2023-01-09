using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ManagementTool.Shared.Utils;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AssignmentsController : ControllerBase {
    

    public IAssignmentDataService AssignmentsDataService { get; }
    public IProjectDataService ProjectDataService { get; }
    public IUserDataService UserDataService { get; }


    public AssignmentsController(IAssignmentDataService assignmentsService, IProjectDataService projectService, 
        IUserDataService userService) {
        AssignmentsDataService = assignmentsService;
        ProjectDataService = projectService;
        UserDataService = userService;
    }

    
    [HttpGet]
    public IEnumerable<Assignment> GetAllAssignments() {
        //todo auth etc.


        return AssignmentsDataService.GetAllAssignments();
    }
    
    [HttpGet("{id}")]
    public IEnumerable<Assignment>? GetAssignmentByUserId(long userId) {

        if (userId < 0) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }
        
        return AssignmentsDataService.GetAssignmentsByUserId(userId);
    }
    
    [HttpPut]
    public long CreateAssignment([FromBody] Assignment assignment) {
        if (assignment == null) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return -1;
        }

        //todo can create assignments for this project?
        var project = ProjectDataService.GetProjectById(assignment.ProjectId);
        if (project == null) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return (int)EAssignmentCreationResponse.InvalidProject;
        }

        var user = UserDataService.GetUserById(assignment.UserId);
        if (user == null) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return (int) EAssignmentCreationResponse.InvalidUser;
        }

        if (!UserDataService.IsUserAssignedToProject(user, project)) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return (int)EAssignmentCreationResponse.UserNotAssignedToProject;
        }

        var valResult = AssignmentUtils.ValidateNewAssignment(assignment, project);
        if (valResult != EAssignmentCreationResponse.Ok){

            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            return (int)valResult;
        }

        //todo possibly chosen in the UI?
        assignment.State = EAssignmentState.Draft;

        var assignmentId = AssignmentsDataService.AddAssignment(assignment);
        if (assignmentId < 0) {
            //Assignment creation failure

            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return -1;
        }

        return assignmentId;
    }

    [HttpDelete("{id}")]
    public void Delete(long id) {

        if (id < 0) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        var done = AssignmentsDataService.DeleteAssignment(id);

        if (!done) {
            //Something failed during deletion
            Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
    }



    /* todo possibly check data conflicts for assignments?
     private EProjectCreationResponse CheckAssignmentDataConflicts(Project project) {
        var allProjects = ProjectDataService.GetAllProjects();
        if (allProjects.Any(existingProject => existingProject.ProjectName.Equals(project.ProjectName)))
        {
            return EProjectCreationResponse.NameTaken;
        }
        return EProjectCreationResponse.Ok;
    }*/
}