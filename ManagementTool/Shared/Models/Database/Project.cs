namespace ManagementTool.Shared.Models.Database;

public class Project
{
    public long Id { get; set; }

    public string ProjectName { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string Description { get; set; }

    public Project(long id, string projectName, DateTime fromDate, DateTime toDate, string description) {
        Id = id;
        ProjectName = projectName;
        FromDate = fromDate;
        ToDate = toDate;
        Description = description;
    }
    public Project(long id, string projectName, DateTime fromDate, DateTime? toDate, string description) {
        Id = id;
        ProjectName = projectName;
        FromDate = fromDate;
        ToDate = toDate;
        Description = description;
    }
}