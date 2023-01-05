namespace ManagementTool.Shared.Models.Database; 

public class UserProjectXRefs {

    public long Id { get; set; }
    public long IdUser { get; set; }
    public long IdProject { get; set; }
    public DateTime? AssignedDate { get; set; }
}