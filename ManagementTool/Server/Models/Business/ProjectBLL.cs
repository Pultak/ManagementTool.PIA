using System.ComponentModel.DataAnnotations;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Server.Models.Business;

/// <summary>
/// Business logic layer project model containing all information
/// our project need with data validation annotations 
/// </summary>
public class ProjectBLL {
    public ProjectBLL() {
        ProjectName = string.Empty;
        Description = string.Empty;
    }

    public ProjectBLL(long id, string projectName, DateTime fromDate, DateTime? toDate, string description) {
        Id = id;
        ProjectName = projectName;
        FromDate = fromDate;
        ToDate = toDate;
        Description = description;
    }

    public long Id { get; set; }

    /// <summary>
    /// Name of the project. Its content should be brief. Abbreviations preferred 
    /// </summary>
    public string ProjectName { get; set; }

    /// <summary>
    /// Date from which this project starts
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// To date on which should this project be done
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Description of the project and its 
    /// </summary>
    public string Description { get; set; }
}