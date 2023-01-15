namespace ManagementTool.Shared.Models.Database;

public class UserSuperiorXRefs {
    public long Id { get; set; }
    public long IdUser { get; set; }
    public long IdSuperior { get; set; }
    public DateTime AssignedDate { get; set; }
}