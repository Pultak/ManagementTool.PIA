namespace ManagementTool.Shared.Models.Utils;

/// <summary>
///     Enum for all possible responses during user creation
/// </summary>
public enum UserCreationResponse {
    EmptyUser,

    //Validation errors
    InvalidUsername,
    InvalidPassword,
    InvalidFullName,
    InvalidEmail,
    InvalidWorkplace,

    //Existing user conflicts
    UsernameTaken,
    Ok
}