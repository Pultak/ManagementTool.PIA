using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ManagementTool.Server.Services.Assignments;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;

namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AssignmentsController : ControllerBase {
    
    private IAuthService AuthService { get; }
    private IAssignmentService AssignmentService { get; }
    private IWorkloadService WorkloadService { get; }

    public AssignmentsController(IAuthService authService, IAssignmentService assignmentService, IWorkloadService workloadService) {
        AuthService = authService;
        AssignmentService = assignmentService;
        WorkloadService = workloadService;
    }

    //todo changed
    [HttpGet("superior")]
    public IEnumerable<AssignmentWrapperPayload>? GetSuperiorsSubordinateAssignments() {
        if (!AuthService.IsUserAuthorized(ERoleType.Superior)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        
        var superiorId = AuthService.GetLoggedUserId();
        if (superiorId == null) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return null;
        }
        var result = AssignmentService.GetAssignmentsUnderSuperior((long)superiorId);
        if (result == null) {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        }else if (result.Any()) {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.NoContent;
        }
        return result;
    }

    //todo changed uri
    [HttpGet("project")]
    public IEnumerable<AssignmentWrapperPayload>? GetProjectSubordinateAssignments() {
        if (!AuthService.IsUserAuthorized(ERoleType.ProjectManager)) {
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
        if (result.Any()) {
            //is the list empty?
            Response.StatusCode = (int)HttpStatusCode.NoContent;
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }
        return result;
    }




    [HttpGet]
    public IEnumerable<AssignmentWrapperPayload>? GetAllAssignments() {
        if (!AuthService.IsUserAuthorized(ERoleType.DepartmentManager)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        Response.StatusCode = (int)HttpStatusCode.OK;
        return AssignmentService.GetAllAssignments();
    }
    
    [HttpGet("my")]
    public IEnumerable<AssignmentWrapperPayload>? GetMyAssignments() {
        var userId = AuthService.GetLoggedUserId();
        if (userId == null) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var result = AssignmentService.GetAssignmentsByUserId((long)userId).ToArray();
        if (result.Any()) {
            Response.StatusCode = (int)HttpStatusCode.NoContent;
            return null;
        }
        Response.StatusCode = (int)HttpStatusCode.OK;
        return result;
    }



    [HttpGet("workloads/{fromDateString}/{toDateString}")]
    public UserWorkloadPayload? GetUsersWorkloads([FromRoute] string fromDateString, string toDateString, [FromQuery] long[] ids) {
        var userRoles = AuthService.GetLoggedUserRoles();
        if (userRoles == null) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        UserWorkloadPayload? resultWorkloads = null;

        var onlyBusinessDays = Request.Query.ContainsKey("businessDays");

        if (AuthService.IsUserAuthorized(ERoleType.DepartmentManager)) {
            //nothing needed to do. Department man can do everything
            resultWorkloads = WorkloadService.GetUsersWorkloads(fromDateString, toDateString, ids,
                projectMan: false, onlyBusinessDays);
        }else if (AuthService.IsUserAuthorized(ERoleType.ProjectManager)) {
            resultWorkloads = WorkloadService.GetUsersWorkloads(fromDateString, toDateString, ids,
                projectMan: true, onlyBusinessDays);
        }

        if (resultWorkloads == null) {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            return null;
        }

        if (resultWorkloads.Dates.Length == 0) {
            Response.StatusCode = (int)HttpStatusCode.NoContent;
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }
        return resultWorkloads;
    }

    //todo changed
    [HttpPost]
    public void CreateAssignment([FromBody] AssignmentPL assignment) {
        if (!AuthService.IsAuthorizedToManageAssignments(assignment.ProjectId)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }
        
        if (AssignmentService.AddNewAssignment(assignment)) {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }else {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        }
    }


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



    
    [HttpDelete("{assignmentId:long}")]
    public void Delete(long assignmentId) {
        if (!AuthService.IsAuthorizedToManageAssignments(assignmentId)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        if (AssignmentService.DeleteAssignment(assignmentId)) {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }else{
            //Something failed during deletion
            Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
        }
    }
}