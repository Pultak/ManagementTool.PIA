using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Models.Business;

/// <summary>
///  Role model from the business logic layer,
/// containing all information our role need 
/// </summary>
public class RoleBLL {
    public RoleBLL() => Name = string.Empty;

    public RoleBLL(long id, string name, RoleType type, long? projectId = null) {
        Id = id;
        Name = name;
        Type = type;
        ProjectId = projectId;
    }

    public long Id { get; set; }

    public string Name { get; set; }


    /// <summary>
    /// Type of role 
    /// </summary>
    public RoleType Type { get; set; }

    /// <summary>
    /// If this is an project manager there is also assigned project id 
    /// otherwise it is null
    /// </summary>
    public long? ProjectId { get; set; }
}