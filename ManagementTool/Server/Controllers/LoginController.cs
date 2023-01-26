using System.Net;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementTool.Server.Controllers;

[Route("api/auth")]
[ApiController, Authorize]
public class LoginController : ControllerBase {
    public LoginController(IAuthService authService) => AuthService = authService;

    private IAuthService AuthService { get; }

    /// <summary>
    /// This endpoint validates the user credentials and send the auth response base on the validity of his request
    /// </summary>
    /// <param name="authRequest">user credentials</param>
    /// <returns>payload with auth response and auth token</returns>
    [HttpPost, AllowAnonymous]
    public AuthResponsePayload Login([FromBody] AuthRequest authRequest) {
        var result = AuthService.Login(authRequest);
        Response.StatusCode = (int)result.statusCode;
        var token = HttpContext.Session.GetString(IAuthService.UserJWTToken);
        var finalResponse = new AuthResponsePayload() {
            Token = token ?? string.Empty,
            Response = result.authResponse
        };
        return finalResponse;
    }
    /*
    /// <summary>
    /// todo
    /// </summary>
    /// <param name="jwtToken"></param>
    /// <returns></returns>
    [HttpPost("token")]
    public AuthResponse Login([FromBody] string jwtToken) {
        var result = AuthService.RenewSessionFromToken(jwtToken);
        Response.StatusCode = (int)result.statusCode;
        return result.authResponse;
    }
    */

    /// <summary>
    /// Endpoint to logout the user from the server 
    /// </summary>
    /// <returns>authResponse enum </returns>
    [HttpGet]
    public AuthResponse Logout() {
        Response.StatusCode = (int)HttpStatusCode.OK;
        return AuthService.Logout();
    }

    /// <summary>
    /// Endpoint for getting the logged in user information needed 
    /// </summary>
    /// <returns></returns>
    [HttpGet("info")]
    public LoggedUserPayload GetLoggedInUser() => AuthService.GetLoggedInUser();


    /// <summary>
    /// Endpoint for changing logged user password.
    /// The password must contain all required characters and muset be of specified length
    /// </summary>
    /// <param name="newPwd">valid new password for user</param>
    [HttpPatch]
    public void LoggedInUserChangePwd([FromBody] string newPwd) {
        Response.StatusCode = (int)AuthService.LoggedInUserChangePwd(newPwd);
    }
}