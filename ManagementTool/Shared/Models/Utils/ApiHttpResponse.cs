namespace ManagementTool.Shared.Models.Utils;

/// <summary>
///     Enum of all possible HTTP response for the POST, PUT and PATCH methods
/// </summary>
public enum ApiHttpResponse {
    Ok,

    /// <summary>
    ///     The response is not yet handled
    /// </summary>
    HttpRequestException,

    /// <summary>
    ///     Arguments you sent are invalid
    /// </summary>
    ArgumentException,

    /// <summary>
    ///     An unknown or unexpected exception was thrown
    /// </summary>
    UnknownException,

    /// <summary>
    ///     Resource that was sent is invalid!
    /// </summary>
    InvalidData,

    /// <summary>
    ///     The resource is already defined in database
    /// </summary>
    ConflictFound,

    /// <summary>
    ///     The response is not yet handled
    /// </summary>
    HttpResponseException,

    /// <summary>
    ///     You are not authorized to access this endpoint
    /// </summary>
    Unauthorized
}