using System.ComponentModel.DataAnnotations;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Shared.Models.Presentation;

/// <summary>
///  Assignment model from the presentation layer, containing all information our assignment need with data validation annotations 
/// </summary>
public class AssignmentPL {
    public AssignmentPL() {
        Id = default;
        ProjectId = default;
        Name = string.Empty;
        Note = string.Empty;
        UserId = default;
        AllocationScope = default;
        FromDate = DateTime.Now;
        ToDate = DateTime.Now.AddDays(1);
        State = AssignmentState.Draft;
    }

    public AssignmentPL(long id, long projectId, string name, string note, long userId,
        long allocationScope, DateTime fromDate, DateTime toDate, AssignmentState state) {
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

    public long Id { get; set; }

    /// <summary>
    /// Id of the project that this assignment is assigned to
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// Name of the assignment. Its content should be brief and detailed info should be inside note
    /// </summary>
    [Required(ErrorMessage = "Jméno úkolu nesmí být prázdný!")]
    [StringLength(AssignmentUtils.MaxAssignmentNameLength, MinimumLength = AssignmentUtils.MinAssignmentNameLength,
        ErrorMessage = "Název úkolu musí být mezi 80 a 3 znaky!")]
    public string Name { get; set; }


    /// <summary>
    /// Note of the assignment.
    /// This could contain some description of the assignment
    /// </summary>
    [Required(ErrorMessage = "Popisek úkolu nesmí být prázdný!")]
    [StringLength(AssignmentUtils.MaxAssignmentNoteLength, MinimumLength = AssignmentUtils.MinAssignmentNameLength,
        ErrorMessage = "Popis úkolu nesmí být delší než 1024 znaků!")]
    public string Note { get; set; }


    /// <summary>
    /// Id of user this assignment is assigned to
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Scope this assignment is allocated to
    /// </summary>
    [Required(ErrorMessage = "Úkol musí mít stanovenou alokaci!")]
    [Range(1, long.MaxValue, ErrorMessage = "Alokace může být pouze v kladných hodnotách!")]
    public long AllocationScope { get; set; }

    /// <summary>
    /// Date from which this assignment starts
    /// </summary>
    [Required(ErrorMessage = "Datum nesmí být prázdný!")]
    [Range(typeof(DateTime), "1/1/2010", "3/4/2999",
        ErrorMessage = "Hodnota pro {0} musí být mezi {1} a {2}")]
    public DateTime FromDate { get; set; }


    /// <summary>
    /// To date on which should this assignment be done
    /// </summary>
    [Required(ErrorMessage = "Datum nesmí být prázdný!")]
    [Range(typeof(DateTime), "1/1/2010", "3/4/2999",
        ErrorMessage = "Hodnota pro {0} musí být mezi {1} a {2}")]
    public DateTime ToDate { get; set; }

    /// <summary>
    /// Current state of the assignment (active/inactive)
    /// </summary>
    public AssignmentState State { get; set; }
}