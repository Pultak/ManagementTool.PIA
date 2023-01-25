namespace ManagementTool.Shared.Models.Utils;

/// <summary>
///     Enum for all possible responses during Assignment creation
/// </summary>
public enum AssignmentCreationResponse {
    /// <summary>
    ///     The assignment value was empty
    /// </summary>
    Empty,

    /// <summary>
    ///     The assigned project to the assignment is invalid
    /// </summary>
    InvalidProject,

    /// <summary>
    ///     Assigned user to the assignment is invalid
    /// </summary>
    InvalidUser,

    /// <summary>
    ///     User is not part of the project and cant be assigned to this assignment
    /// </summary>
    UserNotAssignedToProject,

    /// <summary>
    ///     The name of the assignment is invalid
    /// </summary>
    InvalidName,

    /// <summary>
    ///     The note of the assignment is invalid
    /// </summary>
    InvalidNote,

    /// <summary>
    ///     Entered scope is invalid
    /// </summary>
    InvalidAllocationScope,

    /// <summary>
    ///     From date is in invalid format / or is before 2010
    /// </summary>
    InvalidFromDate,

    /// <summary>
    ///     To date is in invalid format or is before from date
    /// </summary>
    InvalidToDate,

    Ok
}