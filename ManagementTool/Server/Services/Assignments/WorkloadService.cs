using System.Globalization;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Server.Services.Assignments;

public class WorkloadService : IWorkloadService {
    public WorkloadService(IProjectRepository projectRepository, IUserRepository userRepository,
        IAssignmentRepository assignmentRepository, IAuthService authService) {
        ProjectRepository = projectRepository;
        AuthService = authService;
        UserRepository = userRepository;
        AssignmentRepository = assignmentRepository;
    }

    private IProjectRepository ProjectRepository { get; }
    private IAuthService AuthService { get; }
    private IUserRepository UserRepository { get; }
    private IAssignmentRepository AssignmentRepository { get; }


    /// <summary>
    /// Method collects all assignments info from the data source
    /// and creates intersection of them under specified time scope
    /// </summary>
    /// <param name="fromDateString">start time of the time scope</param>
    /// <param name="toDateString">end time of the time scope</param>
    /// <param name="ids">all users you want your intersection calculated for</param>
    /// <param name="projectMan">flag indicating if it is project manager that is requesting it</param>
    /// <param name="onlyBusinessDays">flag indicating if only business days should intersected</param>
    /// <returns>resulting list of all dates and workloads for every user collected</returns>
    public UserWorkloadPayload? GetUsersWorkloads(string fromDateString, string toDateString, long[] ids,
        bool projectMan, bool onlyBusinessDays) {
        DateTime fromDate;
        DateTime toDate;
        try {
            fromDate = DateTime.ParseExact(fromDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            toDate = DateTime.ParseExact(toDateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        }
        catch (ArgumentNullException) {
            //non valid string
            return null;
        }
        catch (FormatException) {
            //non valid date format
            return null;
        }


        if (AssignmentUtils.ValidateWorkloadRequest(ids, fromDate, toDate) != WorkloadValidation.Ok) {
            //request is not valid!
            return null;
        }

        if (projectMan) {
            var managerRoles = AuthService.GetAllProjectManagerRoles();
            if (managerRoles == null) {
                return null;
            }

            var projectIds = managerRoles.Select(x => x.ProjectId).OfType<long>().ToArray();
            if (!ProjectRepository.AreUsersUnderProjects(ids, projectIds)) {
                return null;
            }
        }

        var users = UserRepository.GetUsersById(ids);
        var assignments = AssignmentRepository.GetAllUsersAssignments(ids);

        var days = AssignmentUtils.Split(fromDate, toDate).ToArray();
        if (onlyBusinessDays) {
            //remove weekends if requested
            days = days.Where(x => x.DayOfWeek != DayOfWeek.Sunday
                                   && x.DayOfWeek != DayOfWeek.Saturday).ToArray();
        }

        return ParseDataIntoWorkload(assignments, days, onlyBusinessDays, users);
    }

    /// <summary>
    /// Calculates intersection for every assignment under specified users
    /// the calculated workload means the usability in a day (8 hours) if the workload = 1 user is fully encumbered 
    /// </summary>
    /// <param name="assignments">assignment you want an intersection of</param>
    /// <param name="days">days that should be inside the intersection</param>
    /// <param name="onlyBusinessDays">flag indicating if only business days should be validated</param>
    /// <param name="users"></param>
    /// <returns></returns>
    private UserWorkloadPayload ParseDataIntoWorkload(IEnumerable<AssignmentBLL> assignments, DateTime[] days,
        bool onlyBusinessDays, IEnumerable<UserBaseBLL> users) {
        var firstDate = days.First();
        var lastDate = days.Last();

        var selectedDaysCount = days.Length;

        var workloadDict =
            users.ToDictionary(user => user.Id, user => new UserWorkload(user.FullName, selectedDaysCount));

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
            var load = (double)assignment.AllocationScope / dayCount / 8.0;

            var firstDayMatchIndex = AssignmentUtils.GetDayIndex(days, assignment.FromDate);
            if (firstDayMatchIndex < 0) {
                //the first date was not inside of our scope => We need to start from the first index
                firstDayMatchIndex = 0;
            }

            for (var i = firstDayMatchIndex; i < selectedDaysCount; i++) {
                currentUserWorkload.AllWorkload[i] += load;
                if (assignment.State == AssignmentState.Active) {
                    currentUserWorkload.ActiveWorkload[i] += load;
                }

                if (DateTime.Compare(days[i].Date, assignment.ToDate.Date) >= 0) {
                    //we reached the to date
                    break;
                }
            }
        }

        var result = new UserWorkloadPayload {
            Workloads = workloadDict.Select(x => x.Value).ToArray(),
            Dates = days
        };
        return result;
    }
}