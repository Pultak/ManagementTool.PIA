using ManagementTool.Shared.Models.Utils;
namespace ManagementTool.Server.Models.Database;

public class AssignmentDAL {

    public long Id { get; set; }

    public long ProjectId { get; set; }

    public string Name { get; set; }

    public string Note { get; set; }

    public long UserId { get; set; }

    public long AllocationScope { get; set; }
    
    public DateTime FromDate { get; set; }
    
    public DateTime ToDate { get; set; }
    
    public EAssignmentState State { get; set; }


    public AssignmentDAL() {
        Id = default;
        ProjectId = default;
        Name = string.Empty;
        Note = string.Empty;
        UserId = default;
        AllocationScope = default;
        FromDate = DateTime.Now;
        ToDate = DateTime.Now.AddDays(1);
        State = EAssignmentState.Active;
    }

    public AssignmentDAL(long id, long projectId, string name, string note, long userId, 
        long allocationScope, DateTime fromDate, DateTime toDate, EAssignmentState state) {
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
}