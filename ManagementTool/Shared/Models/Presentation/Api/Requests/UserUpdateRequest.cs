namespace ManagementTool.Shared.Models.Presentation.Api.Requests;

/// <summary>
///     Payload for the update and creation of the users.
/// </summary>
public class UserUpdateRequest {

    public UserUpdateRequest() {

    }
    public UserUpdateRequest(UserBasePL updatedUser) {
        UpdatedUser = updatedUser;
        AssignedRoles = new List<DataModelAssignmentPL<RolePL>>();
        Superiors = new List<UserBasePL>();
    }

    public UserUpdateRequest(UserBasePL updatedUser, List<DataModelAssignmentPL<RolePL>> assignedRoles,
        List<UserBasePL> superiors) : this(updatedUser) {
        UpdatedUser = updatedUser;
        AssignedRoles = assignedRoles;
        Superiors = superiors;
    }

    /// <summary>
    ///     User of which the data were updated and are ready to be changed in the DB
    /// </summary>
    public UserBasePL UpdatedUser { get; set; } = new();

    /// <summary>
    ///     All assigned roles to the user
    /// </summary>
    public List<DataModelAssignmentPL<RolePL>> AssignedRoles { get; set; } = new();

    /// <summary>
    ///     All assigned superiors to the user
    /// </summary>
    public List<UserBasePL> Superiors { get; set; } = new();
}