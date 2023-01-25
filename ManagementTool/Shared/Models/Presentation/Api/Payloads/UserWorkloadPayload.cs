namespace ManagementTool.Shared.Models.Presentation.Api.Payloads;

/// <summary>
///     Wrapper for the workloads and dates so it can be easily visualized
/// </summary>
public class UserWorkloadPayload {
    /// <summary>
    ///     All selected users and their workloads
    /// </summary>
    public UserWorkload[] Workloads { get; set; } = Array.Empty<UserWorkload>();

    /// <summary>
    ///     All selected dates from the desired scope
    /// </summary>
    public DateTime[] Dates { get; set; } = Array.Empty<DateTime>();
}