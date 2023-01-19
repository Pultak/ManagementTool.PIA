namespace ManagementTool.Shared.Models.Api.Payloads;


/// <summary>
/// Wrapper for the workloads and dates so it can be easily visualized
/// </summary>
public class UserWorkloadWrapper
{
    /// <summary>
    /// All selected users and their workloads
    /// </summary>
    public UserWorkload[] Workloads { get; set; }
    /// <summary>
    /// All selected dates from the desired scope
    /// </summary>
    public DateTime[] Dates { get; set; }
}