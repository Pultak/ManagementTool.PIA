using System.Net;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Login;
using Microsoft.AspNetCore.Mvc;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers;
//todo changed
[Route("api/auth")]
[ApiController]
public class LoginController : ControllerBase {
    private IAuthService AuthService { get; }

    public LoginController(IAuthService authService) {
        AuthService = authService;
    }
    
    [HttpPost]
    public AuthResponse Login([FromBody] AuthPayload authPayload) {
        var result = AuthService.Login(authPayload);
        Response.StatusCode = (int)result.statusCode;
        return result.authResponse;
    }

    [HttpGet]
    public AuthResponse Logout() {
        Response.StatusCode = (int)HttpStatusCode.OK;
        return AuthService.Logout();
    }

    [HttpGet("info")]
    public LoggedUserPayload? GetLoggedInUser() {
        return AuthService.GetLoggedInUser();
    }

    //todo
    [HttpPatch]
    public void LoggedInUserChangePwd([FromBody] string newPwd) {
        Response.StatusCode = (int)AuthService.LoggedInUserChangePwd(newPwd);
    }
}