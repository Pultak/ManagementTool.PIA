using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Models.Database;

/// <summary>
///  Role model from the data access layer,
/// containing all information our role need 
/// </summary>
public class RoleDAL {
    public RoleDAL() => Name = string.Empty;

    public RoleDAL(long id, string name, RoleType type, long? projectId = null) {
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