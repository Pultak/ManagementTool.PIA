using ManagementTool.Shared.Models.Api.Payloads;
using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Shared.Models.Api.Requests;

/// <summary>
/// Payload for the update and creation of the users.
/// </summary>
/// <typeparam name="T">Type of user model. UserBase for common and User for creation of new user with generated password</typeparam>
public class UserUpdatePayload<T> where T : UserBase
{

    /// <summary>
    /// User of which the data were updated and are ready to be changed in the DB
    /// </summary>
    public T UpdatedUser { get; set; }

    /// <summary>
    /// All assigned roles to the user
    /// </summary>
    public List<DataModelAssignment<Role>> AssignedRoles { get; set; }

    /// <summary>
    /// All assigned superiors to the user
    /// </summary>
    public List<UserBase> Superiors { get; set; }


    public UserUpdatePayload()
    {

    }

    public UserUpdatePayload(T updatedUser)
    {
        UpdatedUser = updatedUser;
        AssignedRoles = new List<DataModelAssignment<Role>>();
        Superiors = new List<UserBase>();
    }

    public UserUpdatePayload(T updatedUser, List<DataModelAssignment<Role>> assignedRoles, List<UserBase> superiors) : this(updatedUser)
    {
        AssignedRoles = assignedRoles;
        Superiors = superiors;
    }
}