using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Shared.Models.UserComm; 

public class RoleAssignment {
    
    public bool IsAssigned { get; set; }
    public Role Role { get; set; }

    public RoleAssignment() { }

    public RoleAssignment(bool assigned, Role role) {
        IsAssigned = assigned;
        Role = role;
    }
}