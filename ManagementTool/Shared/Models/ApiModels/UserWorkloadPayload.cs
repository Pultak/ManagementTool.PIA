namespace ManagementTool.Shared.Models.ApiModels; 

public class UserWorkloadPayload {
    public UserWorkload[] Workloads { get; set; }
    public DateTime[] Dates { get; set; }
}