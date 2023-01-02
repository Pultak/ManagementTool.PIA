namespace ManagementTool.Shared.Models.Utils; 

public enum EUserCreationResponse {
    EmptyUser, 
    //Validation errors
    InvalidUsername, InvalidPassword, InvalidFullName, InvalidEmail, InvalidWorkplace,
    //Existing user conflicts
    UsernameTaken,   
    Ok
}