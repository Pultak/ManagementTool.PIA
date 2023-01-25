using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Shared.Models.Presentation;

/// <summary>
///  Role model from the presentation layer,
/// containing all information our role need
/// </summary>
public class RolePL {
    public RolePL() => Name = string.Empty;

    public RolePL(long id, string name, RoleType type, long? projectId = null) {
        Id = id;
        Name = name;
        Type = type;
        ProjectId = projectId;
    }

    public long Id { get; set; }

    public string Name { get; set; }

    public RoleType Type { get; set; }

    /// <summary>
    /// If this is an project manager there is also assigned project id 
    /// otherwise it is null
    /// </summary>
    public long? ProjectId { get; set; }
}