namespace ManagementTool.Shared.Models.Api.Requests;

/// <summary>
/// Request wrapper for the API selection of users workloads
/// </summary>
public class WorkloadRequestRequest {

    /// <summary>
    /// Selected users for which the workloads will be calculated
    /// </summary>
    List<long> UserIds { get; set; }

    /// <summary>
    /// First date that should be part of the workload
    /// </summary>
    private DateTime FromDate { get; set; }
    /// <summary>
    /// Last date that should be part of the workload
    /// </summary>
    private DateTime ToDate { get; set; }

}