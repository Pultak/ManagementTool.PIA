using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Shared.Models.UserComm;

public class UserUpdatePayload <T> where T: UserBase{

    public T UpdatedUser { get; set; }

    public List<RoleAssignment> assignedRoles { get; set; }

}