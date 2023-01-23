namespace ManagementTool.Shared.Models.Presentation.Api.Requests;

/// <summary>
/// Payload for the creation of users.
/// </summary>
public class UserCreationRequest : UserUpdateRequest {
    
    public string Pwd { get; set; }
    
    public UserCreationRequest(UserBasePL newUser) : base(newUser) {
        Pwd = string.Empty;
    }

    public UserCreationRequest(UserBasePL newUser, List<DataModelAssignmentPL<RolePL>> assignedRoles, 
        List<UserBasePL> superiors, string pwd) : base(newUser, assignedRoles, superiors) {
        Pwd = pwd;
    }
}