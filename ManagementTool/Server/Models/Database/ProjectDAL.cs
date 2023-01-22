using System.ComponentModel.DataAnnotations;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Server.Models.Database;

public class ProjectDAL{

    public long Id { get; set; }
    
    [Required(ErrorMessage = "Projektové jméno musí být vyplněno!")]
    [StringLength(ProjectUtils.MaxProjectNameLength, MinimumLength = ProjectUtils.MinProjectNameLength, 
        ErrorMessage = "Název projektu musí být mezi 80 a 3 znaky!")]
    public string ProjectName { get; set; }

    [Required(ErrorMessage = "Datum nesmí být prázdný!")]
    [Range(typeof(DateTime), "01/01/2010", "03/04/2999",
        ErrorMessage = "Hodnota pro {0} musí být mezi {1} a {2}")]
    public DateTime FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    [Required(ErrorMessage = "Popisek nesmí být prázdný!")]
    [StringLength(1024, ErrorMessage = "Popis projektu nesmí být delší než 1024 znaků!", MinimumLength = 1)]
    public string Description { get; set; }


    public ProjectDAL() {
        ProjectName = string.Empty;
        Description = string.Empty;
    }

    public ProjectDAL(long id, string projectName, DateTime fromDate, DateTime? toDate, string description) {
        Id = id;
        ProjectName = projectName;
        FromDate = fromDate;
        ToDate = toDate;
        Description = description;
    }


}