namespace ManagementTool.Shared.Models.Utils; 

public enum EAssignmentCreationResponse {

    Empty,

    InvalidProject, InvalidUser,

    UserNotAssignedToProject,

    InvalidName, InvalidNote, InvalidAllocationScope, InvalidFromDate, InvalidToDate, 

    Ok


}