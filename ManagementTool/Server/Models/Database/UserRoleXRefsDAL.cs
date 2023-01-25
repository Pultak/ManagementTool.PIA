namespace ManagementTool.Server.Models.Database;

/// <summary>
/// Database model for table containing roles assigned to users
/// </summary>
public class UserRoleXRefsDAL {
    public long Id { get; set; }
    public long IdUser { get; set; }
    public long IdRole { get; set; }
    /// <summary>
    /// Date on which this superior assignment was made
    /// </summary>
    public DateTime AssignedDate { get; set; }
}