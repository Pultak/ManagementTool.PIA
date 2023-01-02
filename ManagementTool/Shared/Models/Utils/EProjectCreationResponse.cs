namespace ManagementTool.Shared.Models.Utils; 

public enum EProjectCreationResponse {
    EmptyProject, 
    
    InvalidName, InvalidFromDate, InvalidToDate, InvalidDescription,
    
    NameTaken,

    Ok
}