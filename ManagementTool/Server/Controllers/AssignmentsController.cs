using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Mvc;

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


    // GET: api/<AssignmentsController>
    [HttpGet]
    public IEnumerable<Assignment> GetAllAssignments() {
        //todo auth etc.


        return AssignmentsDataService.GetAllAssignments();
    }

    // GET api/<AssignmentsController>/5
    [HttpGet("{id}")]
    public IActionResult GetAssignmentByUserId(long userId) {

        if (userId < 0) {
            return BadRequest();
        }
        
        return Ok(AssignmentsDataService.GetAssignmentsByUserId(userId));
    }

    // POST api/<AssignmentsController>
    [HttpPost]
    public IActionResult CreateAssignment([FromBody] Assignment assignment) {
        if (assignment == null) {
            return BadRequest(EAssignmentCreationResponse.Empty);
        }

        //todo can create assignments for this project?
        var project = ProjectDataService.GetProjectById(assignment.ProjectId);
        if (project == null) {
            return BadRequest(EAssignmentCreationResponse.InvalidProject);
        }

        var user = UserDataService.GetUserById(assignment.UserId);
        if (user == null) {
            return BadRequest(EAssignmentCreationResponse.InvalidUser);
        }

        if (!UserDataService.IsUserAssignedToProject(user, project)) {
            return BadRequest(EAssignmentCreationResponse.UserNotAssignedToProject);
        }

        var valResult = ValidateNewAssignment(assignment, project);
        if (valResult != EAssignmentCreationResponse.Ok){
            return UnprocessableEntity(valResult);
        }

        //todo possibly chosen in the UI?
        assignment.State = EAssignmentState.Draft;

        var assignmentId = AssignmentsDataService.AddAssignment(assignment);
        if (assignmentId < 0) {
            //Assignment creation failure
            return StatusCode(500);
        }

        return Ok(assignmentId);
    }

    // PUT api/<AssignmentsController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value) {
    }

    // DELETE api/<AssignmentsController>/5
    [HttpDelete("{id}")]
    public IActionResult Delete(long id) {

        if (id < 0) {
            return BadRequest();
        }

        var done = AssignmentsDataService.DeleteAssignment(id);

        if (!done) {
            //Something failed during deletion
            return NotFound();
        }

        return Ok();
    }


    private EAssignmentCreationResponse ValidateNewAssignment(Assignment assignment, Project project) {

        if (assignment.Name.Length is < 2 or > 256){
            return EAssignmentCreationResponse.InvalidName;
        }
        
        if (assignment.Note.Length < 1) {
            return EAssignmentCreationResponse.InvalidNote;
        }

        var timeDiff = DateTime.Compare(project.FromDate, assignment.FromDate);
        if (timeDiff < 0) {
            //assignment earlier than the project start date! this is not allowed!
            return EAssignmentCreationResponse.InvalidFromDate;
        }
        
        timeDiff = DateTime.Compare(assignment.FromDate, assignment.ToDate);
        if (timeDiff <= 0){
            //toDate is earlier or from the same time as fromDate
            return EAssignmentCreationResponse.InvalidToDate;
        }
        //todo max allocation?
        if (assignment.AllocationScope < 1) {
            return EAssignmentCreationResponse.InvalidAllocationScope;
        }

        return EAssignmentCreationResponse.Ok;
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