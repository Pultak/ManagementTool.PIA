using ManagementTool.Shared.Models.Api.Payloads;

namespace ManagementTool.Server.Services.Assignments; 

public interface IWorkloadService {

    public UserWorkloadWrapper? GetUsersWorkloads(string fromDateString, string toDateString, long[] ids,
        bool projectMan, bool onlyBusinessDays);

}