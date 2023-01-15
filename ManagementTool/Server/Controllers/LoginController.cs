using System.Net;
using ManagementTool.Server.Services;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
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
        new(1, "Secretariat", ERoleType.Secretariat, null),
        new(2, "Superior", ERoleType.Superior, null),
        new(2, "DepartmentManager", ERoleType.DepartmentManager, null),
        new(2, "TEST1ProjectManager", ERoleType.ProjectManager, 1),
        new(2, "TEST2ProjectManager", ERoleType.ProjectManager, 2),
    };



    [HttpPost]
    public EAuthResponse Login([FromBody] AuthPayload authPayload) {
        if (authPayload == null || string.IsNullOrEmpty(authPayload.Password) 
                                || string.IsNullOrEmpty(authPayload.Username)) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return EAuthResponse.BadRequest;
        }
        if (Thread.CurrentPrincipal != null){
            //already logged in
            return EAuthResponse.AlreadyLoggedIn;
        }

        var user = UserDataService.GetUserByName(authPayload.Username);
        if (user == null) {
            return EAuthResponse.UnknownUser;
        }

        var hashedPwd = HashPwd(authPayload.Password, Convert.FromBase64String(user.Salt));

        if (!user.Pwd.Equals(hashedPwd)) {
            return EAuthResponse.WrongPassword;
        }
        
        var roles = UserRoleDataService.GetUserRolesByUserId(user.Id);

        var rolesArray = roles as Role[] ?? roles.ToArray();
        
        HttpContext.Session.SetInt32("_Id", (int)user.Id);
        HttpContext.Session.SetString("_Username", user.Username);
        HttpContext.Session.SetString("_FullName", user.FullName);
        HttpContext.Session.SetObject("_Roles", rolesArray);
        HttpContext.Session.SetInt32("_InitPwd", user.PwdInit? 1 : 0);
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

        HttpContext.Session.SetInt32("_Id", 1);
        HttpContext.Session.SetString("_Username", "DummyOrion");
        HttpContext.Session.SetString("_FullName", "Dummy Full Name");
        HttpContext.Session.SetObject("_Roles", DummyRoles);
        HttpContext.Session.SetInt32("_InitPwd", 1);
        return new LoggedUserPayload("DummyOrion", "Dummy Full Name", DummyRoles, true);


        var name = HttpContext.Session.GetString("_Username");
        if (name == null) {
            return new LoggedUserPayload(null, null, null, false);
        }
        
        return new LoggedUserPayload(HttpContext.Session.GetString("_Username"), HttpContext.Session.GetString("_FullName"),
            HttpContext.Session.GetObject<Role[]>("_Roles"), HttpContext.Session.GetInt32("_InitPwd") == 1);
    }

    
    [HttpPatch("changePwd")]
    public void LoggedInUserChangePwd([FromBody] string newPwd) {

        var userId = HttpContext.Session.GetInt32("_Id");
        if (userId == null) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }
        
        var user = UserDataService.GetUserById((long)userId);
        if (user == null) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }

        user.Pwd = HashPwd(newPwd, Convert.FromBase64String(user.Salt));
        var ok = UserDataService.UpdateUserPwd(user);
    }


    public static string HashPwd(string password, byte[] salt) {
        // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
        var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        return hashed;
    }



    public static Role[]? GetLoggedUserRoles(ISession session) {
        return session.GetObject<Role[]>("_Roles");
    }
    
    public static bool IsUserAuthorized(ERoleType? neededRole, ISession session) {
        var roles = GetLoggedUserRoles(session);

        return IsUserAuthorized(neededRole, roles);
    }
    public static bool IsUserAuthorized(ERoleType? neededRole, Role[]? roles) {
        return roles != null && (neededRole == null ||
                                 //no one is logged in
                                 roles.Any(role => role.Type.Equals(neededRole))
            );
    }

    public static Role[]? GetAllProjectManagerRoles(ISession session) {

        var roles = GetLoggedUserRoles(session);
        if (roles == null) {
            return null;
        }
        return GetAllProjectManagerRoles(roles);
    }

    public static Role[] GetAllProjectManagerRoles(Role[] roles) {
        return roles.Where(role => role.Type == ERoleType.ProjectManager).ToArray();
    }
    public static long? GetLoggedUserId(ISession session) {
        return session.GetInt32("_Id");
    }


}