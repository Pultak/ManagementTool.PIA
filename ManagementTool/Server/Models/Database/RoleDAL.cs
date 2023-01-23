﻿using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Models.Database;

public class RoleDAL {

    public long Id { get; set; }

    public string Name { get; set; }

    public ERoleType Type { get; set; }

    public long? ProjectId { get; set; }

    public RoleDAL() {
        Name = string.Empty;
    } 

    public RoleDAL(long id, string name, ERoleType type, long? projectId = null) {
        Id = id;
        Name = name;
        Type = type;
        ProjectId = projectId;
    }
}