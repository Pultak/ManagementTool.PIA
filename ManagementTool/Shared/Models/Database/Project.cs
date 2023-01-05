namespace ManagementTool.Shared.Models.Database;

public class Project
{
    public long Id { get; set; }

    public string ProjectName { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string Description { get; set; }

}