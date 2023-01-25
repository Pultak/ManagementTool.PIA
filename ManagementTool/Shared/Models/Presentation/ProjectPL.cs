using System.ComponentModel.DataAnnotations;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Shared.Models.Presentation;

/// <summary>
/// Presentation layer project model containing all information
/// our project need with data validation annotations 
/// </summary>
public class ProjectPL {
    public ProjectPL() {
        ProjectName = string.Empty;
        Description = string.Empty;
    }

    public ProjectPL(long id, string projectName, DateTime fromDate, DateTime? toDate, string description) {
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
    [Required(ErrorMessage = "Projektové jméno musí být vyplněno!")]
    [StringLength(ProjectUtils.MaxProjectNameLength, MinimumLength = ProjectUtils.MinProjectNameLength,
        ErrorMessage = "Název projektu musí být mezi 80 a 3 znaky!")]
    public string ProjectName { get; set; }


    /// <summary>
    /// Date from which this project starts
    /// </summary>
    [Required(ErrorMessage = "Datum nesmí být prázdný!")]
    [Range(typeof(DateTime), "01/01/2010", "03/04/2999",
        ErrorMessage = "Hodnota pro {0} musí být mezi {1} a {2}")]
    public DateTime FromDate { get; set; }

    /// <summary>
    /// To date on which should this project be done
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Description of the project and its 
    /// </summary>
    [Required(ErrorMessage = "Popisek nesmí být prázdný!")]
    [StringLength(ProjectUtils.MaxProjectDescriptionLength, ErrorMessage = "Popis projektu nesmí být delší než 2048 znaků!", MinimumLength = ProjectUtils.MinProjectNameLength)]
    public string Description { get; set; }
}