using System.Net;
using System.Security.Principal;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models;
using ManagementTool.Shared.Models.Login;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers; 

[Route("auth")]
[ApiController]
public class LoginController : ControllerBase {
    public IUserDataService UserDataService { get; }
    public IUserRoleDataService UserRoleDataService{ get; }
    public LoginController(IUserDataService userService, IUserRoleDataService roleService) {
        UserDataService = userService;
        UserRoleDataService = roleService;
    }

    
    [HttpPost]
    public EAuthResponse Login([FromBody] AuthPayload authPayload) {
        if (authPayload == null || string.IsNullOrEmpty(authPayload.Password) 
                                || string.IsNullOrEmpty(authPayload.Username)) {
            throw new BadHttpRequestException("Payload and its data cant be null!");
        }
        //todo check if Principal works as designed
        if (Thread.CurrentPrincipal != null){
            //already logged in
            return EAuthResponse.AlreadyLoggedIn;
        }

        var user = UserDataService.GetUserByName(authPayload.Username);
        if (user == null) {
            return EAuthResponse.UnknownUser;
        }

        //todo cypher pwd

        if (!user.Pwd.Equals(authPayload.Password)) {
            return EAuthResponse.WrongPassword;
        }

        var roles = UserRoleDataService.GetUserRolesById(user.Id);

        var resultRoles = roles.Select(role => role.Name).ToArray();

        var principal = new GenericPrincipal(new GenericIdentity(user.Username), resultRoles);
        Thread.CurrentPrincipal = principal;
        HttpContext.User = principal;
        return EAuthResponse.Success;
    }

    [HttpGet]
    public EAuthResponse Logout() {
        if (Thread.CurrentPrincipal == null) {
            return EAuthResponse.UnknownUser;
        }
        HttpContext.Session.Clear();
        Thread.CurrentPrincipal = null;

        return EAuthResponse.Success;
    }

    [HttpGet("info")]
    public AuthPayload GetLoggedInUser() {

        if (Thread.CurrentPrincipal == null) {
            return new AuthPayload(null, null);
        }

        var userIdentity = HttpContext.User.Identity;
        return new AuthPayload(userIdentity?.Name, null);
    }
}