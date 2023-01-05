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
}