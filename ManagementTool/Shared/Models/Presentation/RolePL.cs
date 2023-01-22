using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Shared.Models.Presentation;

public class RolePL {

    public long Id { get; set; }

    public string Name { get; set; }

    public ERoleType Type { get; set; }

    public long? ProjectId { get; set; }

    public RolePL() {
        Name = string.Empty;
    } 

    public RolePL(long id, string name, ERoleType type, long? projectId = null) {
        Id = id;
        Name = name;
        Type = type;
        ProjectId = projectId;
    }
}