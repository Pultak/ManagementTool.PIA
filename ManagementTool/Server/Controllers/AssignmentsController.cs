using System.Net;
using ManagementTool.Server.Services.Assignments;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AssignmentsController : ControllerBase {
    public AssignmentsController(IAuthService authService, IAssignmentService assignmentService,
        IWorkloadService workloadService) {
        AuthService = authService;
        AssignmentService = assignmentService;
        WorkloadService = workloadService;
    }

    private IAuthService AuthService { get; }
    private IAssignmentService AssignmentService { get; }
    private IWorkloadService WorkloadService { get; }

    
    /// <summary>
    /// Endpoint for getting all assignments.
    /// Only Department managers can access this resource. 
    /// </summary>
    /// <returns>null on unauthorized, otherwise list of all known assignments</returns>
    [HttpGet]
    public IEnumerable<AssignmentWrapperPayload>? GetAllAssignments() {
        if (!AuthService.IsUserAuthorized(RoleType.DepartmentManager)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        Response.StatusCode = (int)HttpStatusCode.OK;
        return AssignmentService.GetAllAssignments();
    }

    
    /// <summary>
    /// Endpoint for getting all assignments assigned to the currently logged user.
    /// </summary>
    /// <returns>null on unauthorized, otherwise list of all known assignments</returns>
    [HttpGet("my")]
    public IEnumerable<AssignmentWrapperPayload>? GetMyAssignments() {
        var userId = AuthService.GetLoggedUserId();
        if (userId == null) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var result = AssignmentService.GetAssignmentsByUserId((long)userId).ToArray();
        
        Response.StatusCode = (int)HttpStatusCode.OK;
        return result;
    }


    /// <summary>
    /// This endpoint returns all assignments that are assigned to the logged in superior
    /// So only user with superior role can access this endpoint
    /// </summary>
    /// <returns>list of all assignments assigned to superior</returns>
    [HttpGet("superior")]
    public IEnumerable<AssignmentWrapperPayload>? GetSuperiorsSubordinateAssignments() {
        if (!AuthService.IsUserAuthorized(RoleType.Superior)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var superiorId = AuthService.GetLoggedUserId();
        if (superiorId == null) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var result = AssignmentService.GetAssignmentsUnderSuperior((long)superiorId);
        if (result == null) {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }

        return result;
    }

    /// <summary>
    /// This endpoint returns all assignments that are assigned to the logged in project manager
    /// So only user with project manager role can access this endpoint
    /// </summary>
    /// <returns>list of all assignments assigned to projects under logged in project manager</returns>
    [HttpGet("project")]
    public IEnumerable<AssignmentWrapperPayload>? GetProjectSubordinateAssignments() {
        if (!AuthService.IsUserAuthorized(RoleType.ProjectManager)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var userRoles = AuthService.GetLoggedUserRoles();
        if (userRoles == null) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var projectIds = AuthService.GetAllProjectManagerProjectIds(userRoles);

        var result = AssignmentService.GetAssignmentsByProjectIds(projectIds);
        
        Response.StatusCode = (int)HttpStatusCode.OK;
        
        return result;
    }

    

    /// <summary>
    /// Endpoint for getting workloads 
    /// </summary>
    /// <param name="fromDateString">start date of the time scope</param>
    /// <param name="toDateString">end date of the time scope</param>
    /// <param name="ids">ids of all users the user wants workloads of</param>
    /// <returns>list of all workloads and selected days, null on unauthorized</returns>
    [HttpGet("workloads/{fromDateString}/{toDateString}")]
    public UserWorkloadPayload? GetUsersWorkloads([FromRoute] string fromDateString, string toDateString,
        [FromQuery] long[] ids) {
        var userRoles = AuthService.GetLoggedUserRoles();
        if (userRoles == null) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        UserWorkloadPayload? resultWorkloads = null;

        var onlyBusinessDays = Request.Query.ContainsKey("businessDays");

        if (AuthService.IsUserAuthorized(RoleType.DepartmentManager)) {
            //nothing needed to do. Department man can do everything
            resultWorkloads = WorkloadService.GetUsersWorkloads(fromDateString, toDateString, ids,
                false, onlyBusinessDays);
        }
        else if (AuthService.IsUserAuthorized(RoleType.ProjectManager)) {
            resultWorkloads = WorkloadService.GetUsersWorkloads(fromDateString, toDateString, ids,
                true, onlyBusinessDays);
        }

        if (resultWorkloads == null) {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            return null;
        }
        
        Response.StatusCode = (int)HttpStatusCode.OK;
        

        return resultWorkloads;
    }

    /// <summary>
    /// Endpoint assigned to creation of new assignments.
    /// These assignments should have valid variables and no data conflicts should arise
    /// </summary>
    /// <param name="assignment">new assignment object</param>
    [HttpPost]
    public void CreateAssignment([FromBody] AssignmentPL assignment) {
        if (!AuthService.IsAuthorizedToManageAssignments(assignment.ProjectId)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        if (AssignmentService.AddNewAssignment(assignment)) {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        }
    }

    /// <summary>
    /// Endpoint assigned to updating of existing assignments.
    /// Similarly to creation endpoint this one also need valid variables and no data conflicts
    /// </summary>
    /// <param name="assignment">assignment that should be updated</param>
    [HttpPatch]
    public void UpdateAssignment([FromBody] AssignmentPL assignment) {
        if (!AuthService.IsAuthorizedToManageAssignments(assignment.ProjectId)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        var updated = AssignmentService.UpdateAssignment(assignment);
        Response.StatusCode = (int)HttpStatusCode.OK;
        if (!updated) {
            //Assignment update failure
            Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
        }
    }

    /// <summary>
    /// Endpoint for deletion of existing assignment.
    /// If the assignment is non existent in the data source a methodNotAllowed status code is returned
    /// </summary>
    /// <param name="assignmentId">Id of the assignment that should be deleted</param>
    [HttpDelete("{assignmentId:long}")]
    public void Delete(long assignmentId) {
        if (!AuthService.IsAuthorizedToManageAssignments(assignmentId)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        if (AssignmentService.DeleteAssignment(assignmentId)) {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }
        else {
            //Something failed during deletion
            Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
        }
    }
}