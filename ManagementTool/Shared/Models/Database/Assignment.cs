using ManagementTool.Shared.Models.Utils;
using System.ComponentModel.DataAnnotations;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Shared.Models.Database;

public class Assignment {

    public long Id { get; set; }

    public long ProjectId { get; set; }


    [Required(ErrorMessage = "Jméno úkolu nesmí být prázdný!")]
    [StringLength(AssignmentUtils.MaxAssignmentNameLength, MinimumLength = AssignmentUtils.MinAssignmentNameLength,
        ErrorMessage = "Název úkolu musí být mezi 80 a 3 znaky!")]
    public string Name { get; set; }


    [Required(ErrorMessage = "Popisek úkolu nesmí být prázdný!")]
    [StringLength(AssignmentUtils.MaxAssignmentNoteLength, MinimumLength = AssignmentUtils.MinAssignmentNameLength, 
        ErrorMessage = "Popis úkolu nesmí být delší než 1024 znaků!")]
    public string Note { get; set; }

    public long UserId { get; set; }

    [Required(ErrorMessage = "Úkol musí mít stanovenou alokaci!")]
    public long AllocationScope { get; set; }

    [Required(ErrorMessage = "Datum nesmí být prázdný!")]
    [Range(typeof(DateTime), "1/1/2010", "3/4/2999",
        ErrorMessage = "Hodnota pro {0} musí být mezi {1} a {2}")]
    public DateTime FromDate { get; set; }

    [Required(ErrorMessage = "Datum nesmí být prázdný!")]
    [Range(typeof(DateTime), "1/1/2010", "3/4/2999",
        ErrorMessage = "Hodnota pro {0} musí být mezi {1} a {2}")]
    public DateTime ToDate { get; set; }
    
    public EAssignmentState State { get; set; }


    public Assignment() {
        Id = default;
        ProjectId = default;
        Name = "";
        Note = "";
        UserId = default;
        AllocationScope = default;
        FromDate = DateTime.Now;
        ToDate = DateTime.Now.AddDays(1);
        State = EAssignmentState.Active;
    }

    public Assignment(long id, long projectId, string name, string note, long userId, 
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