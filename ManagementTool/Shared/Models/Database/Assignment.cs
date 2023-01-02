using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Shared.Models.Database;

public class Assignment {

    public long Id { get; set; }

    public long ProjectId { get; set; }

    //todo assignment name + assignment note
    public string Name { get; set; }

    public string Note { get; set; }

    public long UserId { get; set; }

    public long AllocationScope { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public EAssignmentState State { get; set; }


    public Assignment(long id, long projectId, string name, string note,
        long userId, long allocationScope, DateTime fromDate, DateTime toDate, EAssignmentState state) {
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