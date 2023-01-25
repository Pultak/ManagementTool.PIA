namespace ManagementTool.Shared.Models.Utils;

/// <summary>
///     Response for the workload request
/// </summary>
public enum WorkloadValidation {
    Ok,
    EmptyUsers,

    InvalidUserId,
    InvalidFromDate,
    InvalidToDate,

    /// <summary>
    ///     Selected scope was´too long to visualize
    /// </summary>
    TooLongScope
}