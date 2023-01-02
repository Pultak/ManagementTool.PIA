using System.Security.Principal;
using ManagementTool.Server.Services.Users;
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
    public IActionResult Login([FromBody] AuthData authData) {
        if (authData == null) {
            return BadRequest();
        }
        //todo check if Principal works as designed
        if (Thread.CurrentPrincipal != null){
            //already logged in
            return StatusCode(205);
        }

        var user = UserDataService.GetUserByName(authData.Username);
        if (user == null) {
            return Unauthorized(EAuthResponse.UnknownUser);
        }

        //todo cypher pwd

        if (!user.Pwd.Equals(authData.Password)) {
            return Unauthorized(EAuthResponse.WrongPassword);
        }

        var roles = UserRoleDataService.GetUserRolesById(user.Id);

        var resultRoles = roles.Select(role => role.Name).ToArray();

        var principal = new GenericPrincipal(new GenericIdentity(user.Username), resultRoles);
        Thread.CurrentPrincipal = principal;
        HttpContext.User = principal;
        return Ok(EAuthResponse.Success);
    }

    [HttpGet]
    public IActionResult Logout() {
        //todo if user not logged in -> check func as designed
        if (Thread.CurrentPrincipal == null) {
            return StatusCode(205);
        }
        HttpContext.Session.Clear();
        Thread.CurrentPrincipal = null;

        return Ok();
    }

    [HttpGet]
    public IActionResult GetLoggedInUser() {

        if (Thread.CurrentPrincipal == null) {
            return Unauthorized();
        }

        var userIdentity = HttpContext.User.Identity;
        if (userIdentity == null) {
            return Unauthorized();
        }
        return Ok(userIdentity.Name);
    }
}