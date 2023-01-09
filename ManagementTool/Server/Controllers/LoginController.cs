using System.Collections;
using System.Net;
using System.Security.Principal;
using ManagementTool.Server.Services;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Utils;
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

    //todo move to tests file
    public static Role[] DummyRoles = {
        new Role(1, "Secretariat", ERoleType.Secretariat, null),
        new Role(2, "Superior", ERoleType.Superior, null),
        new Role(2, "DepartmentManager", ERoleType.DepartmentManager, null),
        new Role(2, "TEST1ProjectManager", ERoleType.ProjectManager, 1),
        new Role(2, "TEST2ProjectManager", ERoleType.ProjectManager, 2),
    };



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

        var rolesArray = roles as Role[] ?? roles.ToArray();
        var resultRoles = rolesArray.Select(role => role.Name).ToArray();

        /* todo remove old version
         var principal = new GenericPrincipal(new GenericIdentity(user.Username), resultRoles);
        Thread.CurrentPrincipal = principal;
        HttpContext.User = principal;
        */
        HttpContext.Session.SetString("_Username", user.Username);
        HttpContext.Session.SetString("_FullName", user.FullName);
        HttpContext.Session.SetObject("_Roles", rolesArray);
        return EAuthResponse.Success;
    }

    [HttpGet]
    public EAuthResponse Logout() {
        var name = HttpContext.Session.GetString("_Username");
        if (name == null) {
            return EAuthResponse.UnknownUser;
        }
        HttpContext.Session.Clear();
        Thread.CurrentPrincipal = null;

        return EAuthResponse.Success;
    }

    [HttpGet("info")]
    public LoggedUserPayload GetLoggedInUser() {
        //todo remove latter

        return new LoggedUserPayload("DummyOrion", "Dummy Full Name", DummyRoles);


        var name = HttpContext.Session.GetString("_Username");
        if (name == null) {
            return new LoggedUserPayload(null, null, null);
        }
        
        return new LoggedUserPayload(HttpContext.Session.GetString("_Username"), HttpContext.Session.GetString("_FullName"),
            HttpContext.Session.GetObject<Role[]>("_Roles"));
    }
}