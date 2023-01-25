namespace ManagementTool.Server.Models.Database;

/// <summary>
/// Database model for table containing superiors assigned to users
/// </summary>
public class UserSuperiorXRefsDAL {
    public long Id { get; set; }
    public long IdUser { get; set; }
    public long IdSuperior { get; set; }
    /// <summary>
    /// Date on which this superior assignment was made
    /// </summary>
    public DateTime AssignedDate { get; set; }
}