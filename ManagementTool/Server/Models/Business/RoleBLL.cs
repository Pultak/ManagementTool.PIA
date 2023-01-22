using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Models.Business;

public class RoleBLL {

    public long Id { get; set; }

    public string Name { get; set; }

    public ERoleType Type { get; set; }

    public long? ProjectId { get; set; }

    public RoleBLL() {
        Name = string.Empty;
    } 

    public RoleBLL(long id, string name, ERoleType type, long? projectId = null) {
        Id = id;
        Name = name;
        Type = type;
        ProjectId = projectId;
    }
}