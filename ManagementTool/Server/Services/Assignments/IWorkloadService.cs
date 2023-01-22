using ManagementTool.Shared.Models.Presentation.Api.Payloads;

namespace ManagementTool.Server.Services.Assignments;

public interface IWorkloadService {

    public UserWorkloadPayload? GetUsersWorkloads(string fromDateString, string toDateString, long[] ids,
        bool projectMan, bool onlyBusinessDays);

}