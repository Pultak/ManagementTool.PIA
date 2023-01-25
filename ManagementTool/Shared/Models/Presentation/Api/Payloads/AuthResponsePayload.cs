using ManagementTool.Shared.Models.Login;

namespace ManagementTool.Shared.Models.Presentation.Api.Payloads; 

public class AuthResponsePayload {

    public AuthResponse Response { get; set; }
    public string Token { get; set; } = string.Empty;
}