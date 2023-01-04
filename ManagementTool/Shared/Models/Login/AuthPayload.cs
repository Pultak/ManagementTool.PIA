namespace ManagementTool.Shared.Models.Login; 

public class AuthPayload {
    public string? Username { get; set; }
    public string? Password { get; set; }
    
    public AuthPayload(string? username, string? password) {
        Username = username;
        Password = password;
    }
}