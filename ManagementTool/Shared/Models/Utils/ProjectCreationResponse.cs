namespace ManagementTool.Shared.Models.Utils;

/// <summary>
///     Enum for all possible responses during Project creation
/// </summary>
public enum ProjectCreationResponse {
    /// <summary>
    ///     The project value was empty
    /// </summary>
    EmptyProject,


    /// <summary>
    ///     The name of the project is invalid
    /// </summary>
    InvalidName,

    /// <summary>
    ///     From date is in invalid format / or is before 2010 / or is earlier than project
    /// </summary>
    InvalidFromDate,

    /// <summary>
    ///     To date is in invalid format or is before from date
    /// </summary>
    InvalidToDate,

    /// <summary>
    ///     The description of the project is invalid
    /// </summary>
    InvalidDescription,

    /// <summary>
    ///     Project with this name already exists
    /// </summary>
    NameTaken,

    /// <summary>
    ///     New role for the new project is invalid
    /// </summary>
    InvalidRoleName,

    Ok
}