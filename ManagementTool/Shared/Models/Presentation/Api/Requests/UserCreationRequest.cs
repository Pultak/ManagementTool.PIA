namespace ManagementTool.Shared.Models.Presentation.Api.Requests;

/// <summary>
///     Payload for the creation of users.
/// </summary>
public class UserCreationRequest : UserUpdateRequest {

    public UserCreationRequest(): base() {

    }
    public UserCreationRequest(UserBasePL newUser) : base(newUser) => Pwd = string.Empty;

    public UserCreationRequest(UserBasePL newUser, List<DataModelAssignmentPL<RolePL>> assignedRoles,
        List<UserBasePL> superiors, string pwd) : base(newUser, assignedRoles, superiors) =>
        Pwd = pwd;

    /// <summary>
    ///     Generated user password that should not be checked for validity
    /// </summary>
    public string Pwd { get; set; } = string.Empty;
}