namespace ManagementTool.Server.Models.Business; 

/// <summary>
/// Model containing data about current session with user 
/// </summary>
public class SessionInfo {
    /// <summary>
    /// logged in user info
    /// </summary>
    public UserBaseBLL User { get; set; } = new();

    /// <summary>
    /// Logged in user's assigned roles
    /// </summary>
    public RoleBLL[] Roles { get; set; } = Array.Empty<RoleBLL>();
}