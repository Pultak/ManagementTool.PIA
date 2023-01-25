using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Models.Database;
/// <summary>
///  Assignment model from the data access layer (DAL),
/// containing all information our assignment need
/// </summary>
public class AssignmentDAL {
    public AssignmentDAL() {
        Id = default;
        ProjectId = default;
        Name = string.Empty;
        Note = string.Empty;
        UserId = default;
        AllocationScope = default;
        FromDate = DateTime.Now;
        ToDate = DateTime.Now.AddDays(1);
        State = AssignmentState.Active;
    }

    public AssignmentDAL(long id, long projectId, string name, string note, long userId,
        long allocationScope, DateTime fromDate, DateTime toDate, AssignmentState state) {
        Id = id;
        ProjectId = projectId;
        Name = name;
        Note = note;
        UserId = userId;
        AllocationScope = allocationScope;
        FromDate = fromDate;
        ToDate = toDate;
        State = state;
    }

    public long Id { get; set; }

    /// <summary>
    /// Id of the project that this assignment is assigned to
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Name of the assignment.
    /// Its content should be brief and detailed info should be inside note
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Note of the assignment.
    /// This could contain some description of the assignment
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// Id of user this assignment is assigned to
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Scope this assignment is allocated to
    /// </summary>
    public long AllocationScope { get; set; }

    /// <summary>
    /// Date from which this assignment starts
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// To date on which should this assignment be done
    /// </summary>
    public DateTime ToDate { get; set; }
    /// <summary>
    /// Current state of the assignment (active/inactive)
    /// </summary>
    public AssignmentState State { get; set; }
}