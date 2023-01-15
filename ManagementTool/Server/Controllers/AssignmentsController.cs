using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ManagementTool.Shared.Models.ApiModels;
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
    public IEnumerable<AssignmentWrapper>? GetAllAccessibleAssignments() {
        var userRoles = LoginController.GetLoggedUserRoles(HttpContext.Session);
        if (userRoles == null) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        Response.StatusCode = (int)HttpStatusCode.OK;
        if (LoginController.IsUserAuthorized(ERoleType.DepartmentManager, userRoles)) {
            return AssignmentsDataService.GetAllAssignments();
        }
        if (LoginController.IsUserAuthorized(ERoleType.ProjectManager, userRoles)) {
            var managerRoles = LoginController.GetAllProjectManagerRoles(userRoles);
            var projectIds = managerRoles.Select(x => x.ProjectId).OfType<long>().ToList();
            return AssignmentsDataService.GetAssignmentsByProjectIds(projectIds);
        }
        
        if (LoginController.IsUserAuthorized(ERoleType.Superior, userRoles)) {
            var superiorId = LoginController.GetLoggedUserId(HttpContext.Session);
            if (superiorId == null) {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return null;
            }
            return AssignmentsDataService.GetAssignmentsUnderSuperior((long)superiorId);
        }
        Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        return null;
    }
    
    [HttpGet("myAssignments")]
    public IEnumerable<AssignmentWrapper>? GetMyAssignments() {
        if (!LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var userId = LoginController.GetLoggedUserId(HttpContext.Session);
        if (userId == null) {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return null;
        }

        var result = AssignmentsDataService.GetAssignmentsByUserId((long)userId);
        Response.StatusCode = (int)HttpStatusCode.OK;
        return result.ToList();
    }


    /* todo remove
    [HttpGet("{id}")]
    public IEnumerable<Assignment>? GetAssignmentByUserId(long userId) {

        if (userId < 0) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }
        
        return AssignmentsDataService.GetAssignmentsByUserId(userId);
    }
    */


    [HttpPut]
    public long CreateAssignment([FromBody] Assignment assignment) {
        if (!IsAuthorizedToManageAssignments(assignment.ProjectId)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return -1;
        }

        if (ValidateAssignmentData(assignment) != EAssignmentCreationResponse.Ok) {
            return -1;
        }
        
        assignment.State = EAssignmentState.Draft;

        var assignmentId = AssignmentsDataService.AddAssignment(assignment);
        if (assignmentId < 0) {
            //Assignment creation failure

            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return -1;
        }

        Response.StatusCode = (int)HttpStatusCode.OK;
        return assignmentId;
    }


    [HttpPatch]
    public void UpdateAssignment([FromBody] Assignment assignment) {

        if (!IsAuthorizedToManageAssignments(assignment.ProjectId)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        if (ValidateAssignmentData(assignment) != EAssignmentCreationResponse.Ok) {
            return;
        }
        
        var updated = AssignmentsDataService.UpdateAssignment(assignment);
        Response.StatusCode = (int)HttpStatusCode.OK;
        if (!updated) {
            //Assignment update failure
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }

    private bool IsAuthorizedToManageAssignments(long relevantProjectId) {
        var userRoles = LoginController.GetLoggedUserRoles(HttpContext.Session);
        if (userRoles == null) {
            return false;
        }
        if (LoginController.IsUserAuthorized(ERoleType.DepartmentManager, userRoles)) {
            //all ok, department manager can do everything
            return true;
        }

        if (!LoginController.IsUserAuthorized(ERoleType.ProjectManager, userRoles)) {
            return false;
        }

        var managerRoles = LoginController.GetAllProjectManagerRoles(userRoles);

        //can this manager manage assignments for this project?
        Response.StatusCode = (int)HttpStatusCode.OK;
        return managerRoles.Any(x => x.ProjectId == relevantProjectId);
    }

    private EAssignmentCreationResponse ValidateAssignmentData(Assignment assignment) {
        
        var project = ProjectDataService.GetProjectById(assignment.ProjectId);
        if (project == null) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return EAssignmentCreationResponse.InvalidProject;
        }

        var user = UserDataService.GetUserById(assignment.UserId);
        if (user == null) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return EAssignmentCreationResponse.InvalidUser;
        }

        if (!UserDataService.IsUserAssignedToProject(user, project)) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return EAssignmentCreationResponse.UserNotAssignedToProject;
        }

        var valResult = AssignmentUtils.ValidateNewAssignment(assignment, project, user);
        if (valResult != EAssignmentCreationResponse.Ok){

            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            return valResult;
        }
        Response.StatusCode = (int)HttpStatusCode.OK;
        return EAssignmentCreationResponse.Ok;
    }

    
    [HttpDelete("{assignmentId:long}")]
    public void Delete(long assignmentId) {
        if (!IsAuthorizedToManageAssignments(assignmentId)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }
        if (assignmentId < 0) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        var done = AssignmentsDataService.DeleteAssignment(assignmentId);
        Response.StatusCode = (int)HttpStatusCode.OK;

        if (!done) {
            //Something failed during deletion
            Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
    }
}