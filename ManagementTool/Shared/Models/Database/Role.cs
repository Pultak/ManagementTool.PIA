using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Shared.Models.Database;

public class Role {

    public long Id { get; set; }

    public string Name { get; set; }

    public ERoleType Type { get; set; }

    public long? ProjectId { get; set; }

    public Role() { } 

    public Role(long id, string name, ERoleType type, long? projectId = null) {
        Id = id;
        Name = name;
        Type = type;
        ProjectId = projectId;
    }
}