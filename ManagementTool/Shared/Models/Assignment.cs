namespace ManagementTool.Shared.Models; 

internal class Assignment {
    public long Id { get; set; }

    public long ProjectId { get; set; }


    public long UserId { get; set; }

    public long AllocationScope { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public EAssignmentState State { get; set; }


}