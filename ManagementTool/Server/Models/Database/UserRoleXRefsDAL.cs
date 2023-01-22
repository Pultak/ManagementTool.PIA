namespace ManagementTool.Server.Models.Database; 

public class UserRoleXRefsDAL {
    public long Id { get; set; }
    public long IdUser { get; set; }
    public long IdRole { get; set; }
    public DateTime AssignedDate { get; set; }
}