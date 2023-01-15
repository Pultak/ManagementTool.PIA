using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Shared.Utils; 

public static class AssignmentUtils {

    public const int MaxAssignmentNameLength = 64;
    public const int MinAssignmentNameLength = 2;

    public const int MaxAssignmentNoteLength = 1024;
    public const int MinAssignmentNoteLength = 1;

    public static EAssignmentCreationResponse ValidateNewAssignment(Assignment assignment, Project? project, User? user) {

        if (project == null || assignment.ProjectId != project.Id) {
            return EAssignmentCreationResponse.InvalidProject;
        }

        if (user == null || assignment.UserId != user.Id) {
            return EAssignmentCreationResponse.InvalidUser;
        }

        if (assignment.Name.Length is < MinAssignmentNameLength or > MaxAssignmentNameLength){
            return EAssignmentCreationResponse.InvalidName;
        }
        
        if (assignment.Note.Length < MinAssignmentNoteLength && assignment.Note.Length > MaxAssignmentNoteLength) {
            return EAssignmentCreationResponse.InvalidNote;
        }

        var timeDiff = DateTime.Compare(project.FromDate, assignment.FromDate);
        if (timeDiff > 0) {
            //assignment earlier than the project start date! this is not allowed!
            return EAssignmentCreationResponse.InvalidFromDate;
        }
        
        timeDiff = DateTime.Compare(assignment.FromDate, assignment.ToDate);
        if (timeDiff >= 0){
            //toDate is earlier or from the same time as fromDate
            return EAssignmentCreationResponse.InvalidToDate;
        }
        if (assignment.AllocationScope < 1) {
            return EAssignmentCreationResponse.InvalidAllocationScope;
        }

        return EAssignmentCreationResponse.Ok;
    }
    
}