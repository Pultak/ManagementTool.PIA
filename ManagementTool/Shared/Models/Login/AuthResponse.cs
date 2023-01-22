namespace ManagementTool.Shared.Models.Login; 

public enum AuthResponse {
    Success, UnknownUser, WrongPassword, AlreadyLoggedIn, UnknownResponse, EmptyUsername, EmptyPassword, BadRequest
        
}