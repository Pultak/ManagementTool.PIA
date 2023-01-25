namespace ManagementTool.Shared.Models.Login;

/// <summary>
///     Auth controller responses on his possible actions
/// </summary>
public enum AuthResponse {
    /// <summary>
    ///     All went well
    /// </summary>
    Success,

    /// <summary>
    ///     The identity of user is unknown for the server
    /// </summary>
    UnknownUser,

    /// <summary>
    ///     The inserted password is invalid with user credentials!
    /// </summary>
    WrongPassword,

    /// <summary>
    ///     User trying to log in. Is already logged in
    /// </summary>
    AlreadyLoggedIn,

    /// <summary>
    ///     There was an unexpected action for which we dont have defined solution
    /// </summary>
    UnknownResponse,

    /// <summary>
    ///     Empty username was sent
    /// </summary>
    EmptyUsername,

    /// <summary>
    ///     Empty password was sent
    /// </summary>
    EmptyPassword,

    /// <summary>
    ///     Request sent to the login controller is invalid
    /// </summary>
    BadRequest
}