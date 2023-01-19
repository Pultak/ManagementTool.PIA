namespace ManagementTool.Shared.Models.ApiModels; 

public class WorkloadRequestPayload {

    List<long> UserIds {get; set; }
    private DateTime FromDate { get; set; }
    private DateTime ToDate { get; set; }

}