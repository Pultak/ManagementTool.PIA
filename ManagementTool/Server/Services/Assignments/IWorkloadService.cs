using ManagementTool.Shared.Models.Presentation.Api.Payloads;

namespace ManagementTool.Server.Services.Assignments;

public interface IWorkloadService {
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
        bool projectMan, bool onlyBusinessDays);
}