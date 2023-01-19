using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Shared.Models.ApiModels;

public class UserUpdatePayload <T> where T: UserBase{

    public T UpdatedUser { get; set; }

    public List<DataModelAssignment<Role>> AssignedRoles { get; set; }

    public List<UserBase> Superiors { get; set; }


    public UserUpdatePayload() {

    }

    public UserUpdatePayload(T updatedUser) {
        UpdatedUser = updatedUser;
        AssignedRoles = new List<DataModelAssignment<Role>>();
        Superiors = new List<UserBase>();
    }

    public UserUpdatePayload(T updatedUser, List<DataModelAssignment<Role>> assignedRoles, List<UserBase> superiors) : this(updatedUser) {
        AssignedRoles = assignedRoles;
        Superiors = superiors;
    }
}