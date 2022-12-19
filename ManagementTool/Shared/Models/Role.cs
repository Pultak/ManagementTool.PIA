namespace ManagementTool.Shared.Models; 

public class Role {

    public long Id { get; set; }

    public string Name { get; set; }

    public ERoleType Type { get; set; }

    public long? ProjectId { get; set; }


    public Role(long id, string name, ERoleType type, long? projectId)
    {
        Id = id;
        Name = name;
        Type = type;
        ProjectId = projectId;
    }

}