namespace ManagementTool.Server.Models.Database;

public class UserSuperiorXRefsDAL {
    public long Id { get; set; }
    public long IdUser { get; set; }
    public long IdSuperior { get; set; }
    public DateTime AssignedDate { get; set; }
}