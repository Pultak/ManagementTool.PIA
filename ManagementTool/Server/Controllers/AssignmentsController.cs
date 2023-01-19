using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ManagementTool.Shared.Models.ApiModels;
using ManagementTool.Shared.Utils;
using System.Globalization;
using ManagementTool.Server.Repository.Projects;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AssignmentsController : ControllerBase {
    

    public IAssignmentRepository AssignmentsRepository { get; }
    public IProjectRepository ProjectRepository { get; }
    public IUserRepository UserRepository { get; }


    public AssignmentsController(IAssignmentRepository assignmentsRepository, IProjectRepository projectService, 
        IUserRepository userService) {
        AssignmentsRepository = assignmentsRepository;
        ProjectRepository = projectService;
        UserRepository = userService;
    }


    [HttpGet("subordinates")]
    public IEnumerable<AssignmentWrapper>? GetSuperiorsSubordinateAssignments() {
        if (!LoginController.IsUserAuthorized(ERoleType.Superior, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        
        var superiorId = LoginController.GetLoggedUserId(HttpContext.Session);
        if (superiorId == null) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return null;
        }
        Response.StatusCode = (int)HttpStatusCode.OK;
        return AssignmentsRepository.GetAssignmentsUnderSuperior((long)superiorId);
    }

    
    [HttpGet("projectSubordinates")]
    public IEnumerable<AssignmentWrapper>? GetProjectSubordinateAssignments() {
        var userRoles = LoginController.GetLoggedUserRoles(HttpContext.Session);
        if (userRoles == null) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        if (!LoginController.IsUserAuthorized(ERoleType.ProjectManager, userRoles)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var managerRoles = LoginController.GetAllProjectManagerRoles(userRoles);
        var projectIds = managerRoles.Select(x => x.ProjectId).OfType<long>().ToList();
        Response.StatusCode = (int)HttpStatusCode.OK;
        return AssignmentsRepository.GetAssignmentsByProjectIds(projectIds);
    }




    [HttpGet]
    public IEnumerable<AssignmentWrapper>? GetAllAssignments() {
        if (!LoginController.IsUserAuthorized(ERoleType.DepartmentManager, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        Response.StatusCode = (int)HttpStatusCode.OK;
        return AssignmentsRepository.GetAllAssignments();
    }
    
    [HttpGet("my")]
    public IEnumerable<AssignmentWrapper>? GetMyAssignments() {
        if (!LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var userId = LoginController.GetLoggedUserId(HttpContext.Session);
        if (userId == null) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return null;
        }

        var result = AssignmentsRepository.GetAssignmentsByUserId((long)userId);
        Response.StatusCode = (int)HttpStatusCode.OK;
        return result.ToList();
    }



    [HttpGet("workload/{fromDateString}/{toDateString}")]
    public UserWorkloadPayload? GetUsersWorkloads([FromRoute] string fromDateString, string toDateString, [FromQuery] long[] ids) {
        var userRoles = LoginController.GetLoggedUserRoles(HttpContext.Session);
        if (userRoles == null) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        DateTime fromDate;
        DateTime toDate;
        try {
            fromDate = DateTime.ParseExact(fromDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            toDate = DateTime.ParseExact(toDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

        }
        catch (ArgumentNullException e) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }
        catch (FormatException e) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }
        

        if (AssignmentUtils.ValidateWorkloadPayload(ids, fromDate, toDate) != EWorkloadValidation.Ok) {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            return null;
        }
        
        if (LoginController.IsUserAuthorized(ERoleType.DepartmentManager, userRoles)) {
            //nothing needed to do. Department man can do everything
        }else if (LoginController.IsUserAuthorized(ERoleType.ProjectManager, userRoles)) {

            var managerRoles = LoginController.GetAllProjectManagerRoles(userRoles);
            var projectIds = managerRoles.Select(x => x.ProjectId).OfType<long>().ToArray();
            if (!ProjectRepository.AreUsersUnderProjects(ids, projectIds)) {
                Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                return null;
            }
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var users = UserRepository.GetUsersById(ids);
        var assignments = AssignmentsRepository.GetAllUsersAssignments(ids);
        
        var days = AssignmentUtils.Split(fromDate, toDate).ToArray();
        var onlyBusinessDays = Request.Query.ContainsKey("businessDays");
        if (onlyBusinessDays) {
            //remove weekends if requested
            days = days.Where(x => x.DayOfWeek != DayOfWeek.Sunday
                                   && x.DayOfWeek != DayOfWeek.Saturday).ToArray();
        }
        Response.StatusCode = (int)HttpStatusCode.OK;
        return ParseDataIntoWorkload(assignments, days, onlyBusinessDays, users);
    }

    private UserWorkloadPayload? ParseDataIntoWorkload(IEnumerable<Assignment> assignments, DateTime[] days,
        bool onlyBusinessDays, IEnumerable<UserBase> users) {

        var firstDate = days.First();
        var lastDate = days.Last();

        var selectedDaysCount = days.Length;

        var workloadDict = users.ToDictionary(user => user.Id, user => new UserWorkload(user.FullName, selectedDaysCount));


        foreach (var assignment in assignments) {
            var timeDiff = DateTime.Compare(firstDate, assignment.ToDate);
            if (timeDiff > 0) {
                //to date is earlier or from the same time => no reason to check workload
                continue;
            }

            timeDiff = DateTime.Compare(lastDate, assignment.FromDate);
            if (timeDiff < 0) {
                //from date is later or from the same time => no reason to check workload
                continue;
            }

            var currentUserWorkload = workloadDict[assignment.UserId];

            int dayCount;

            if (onlyBusinessDays) {
                dayCount = assignment.FromDate.Date.BusinessDaysUntil(assignment.ToDate.Date);
            }
            else {
                dayCount = (assignment.ToDate.Date - assignment.FromDate.Date).Days;
            }
            // divide by 8 to get daily workload (8 work hours)
            var load = ((double)assignment.AllocationScope / (double)dayCount) / 8.0;

            var firstDayMatchIndex = AssignmentUtils.GetDayIndex(days, assignment.FromDate);
            if (firstDayMatchIndex < 0) {
                //the first date was not inside of our scope => We need to start from the first index
                firstDayMatchIndex = 0;
            }

            for (var i = firstDayMatchIndex; i < selectedDaysCount; i++) {
                currentUserWorkload.AllWorkload[i] += load;
                if (assignment.State == EAssignmentState.Active) {
                    currentUserWorkload.ActiveWorkload[i] += load;
                }
                if (DateTime.Compare(days[i].Date, assignment.ToDate.Date) >= 0) {
                    //we reached the to date
                    break;
                }
            }
        }

        var result = new UserWorkloadPayload{
                Workloads = workloadDict.Select(x => x.Value).ToArray(),
                Dates = days

        };
        return result;
    }

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

        var assignmentId = AssignmentsRepository.AddAssignment(assignment);
        if (assignmentId < 0) {
            //Assignment creation failure

            Response.StatusCode = (int)HttpStatusCode.NotFound;
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
        
        var updated = AssignmentsRepository.UpdateAssignment(assignment);
        Response.StatusCode = (int)HttpStatusCode.OK;
        if (!updated) {
            //Assignment update failure
            Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
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
        
        var project = ProjectRepository.GetProjectById(assignment.ProjectId);
        if (project == null) {
            Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            return EAssignmentCreationResponse.InvalidProject;
        }

        var user = UserRepository.GetUserById(assignment.UserId);
        if (user == null) {
            Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            return EAssignmentCreationResponse.InvalidUser;
        }

        if (!UserRepository.IsUserAssignedToProject(user, project)) {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
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
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            return;
        }

        var done = AssignmentsRepository.DeleteAssignment(assignmentId);
        Response.StatusCode = (int)HttpStatusCode.OK;

        if (!done) {
            //Something failed during deletion
            Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
        }
    }
}