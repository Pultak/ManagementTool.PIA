namespace ManagementTool.Server.Models.Database;

/// <summary>
///  Database model for table containing project assigned to user
/// </summary>
public class UserProjectXRefsDAL {
    public long Id { get; set; }
    public long IdUser { get; set; }
    public long IdProject { get; set; }

    /// <summary>
    /// Date on which this superior assignment was made
    /// </summary>
    public DateTime? AssignedDate { get; set; }
}