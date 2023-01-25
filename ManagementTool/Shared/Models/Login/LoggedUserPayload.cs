using ManagementTool.Shared.Models.Presentation;

namespace ManagementTool.Shared.Models.Login;

/// <summary>
///     Payload of logged in user with all his crucial information
/// </summary>
public class LoggedUserPayload {
    public LoggedUserPayload() {
    }

    public LoggedUserPayload(string? userName, string? fullName, RolePL[]? roles, bool hasInitPwd) {
        UserName = userName;
        FullName = fullName;
        Roles = roles;
        HasInitPwd = hasInitPwd;
    }

    /// <summary>
    ///     User name of the logged user. Null if there is no logged in user
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    ///     Full name of the logged user. Null if there is no logged in user
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    ///     User Roles of the logged user. Null if there is no logged in user
    /// </summary>
    public RolePL[]? Roles { get; set; }


    /// <summary>
    ///     Flag indicating that the users password is generated and should be changed asap
    /// </summary>
    public bool HasInitPwd { get; set; }
}