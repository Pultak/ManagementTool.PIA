using System.Net;
using ManagementTool.Server.Services;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers;

[Route("auth")]
[ApiController]
public class LoginController : ControllerBase {
    public const string UserIdKey = "_Id";
    public const string UsernameKey = "_Username";
    public const string UserFullNameKey = "_FullName";
    public const string UserRolesKey = "_Roles";
    public const string UserHasInitPwdKey = "_InitPwd";

    //todo move to tests file
    public static Role[] DummyRoles = {
        new(1, "Secretariat", ERoleType.Secretariat),
        new(2, "Superior", ERoleType.Superior),
        new(2, "DepartmentManager", ERoleType.DepartmentManager),
        new(2, "TEST1ProjectManager", ERoleType.ProjectManager, 1),
        new(2, "TEST2ProjectManager", ERoleType.ProjectManager, 2)
    };

    public LoginController(IUserRepository userService, IUserRoleRepository roleService) {
        UserRepository = userService;
        UserRoleRepository = roleService;
    }


    public IUserRepository UserRepository { get; }
    public IUserRoleRepository UserRoleRepository { get; }


    [HttpPost]
    public EAuthResponse Login([FromBody] AuthPayload authPayload) {
        if (string.IsNullOrEmpty(authPayload.Password) || string.IsNullOrEmpty(authPayload.Username)) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return EAuthResponse.BadRequest;
        }

        if (IsUserAuthorized(null, HttpContext.Session)) {
            //already logged in
            Response.StatusCode = (int)HttpStatusCode.OK;
            return EAuthResponse.AlreadyLoggedIn;
        }

        var user = UserRepository.GetUserByName(authPayload.Username);
        if (user == null) {
            //Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return EAuthResponse.UnknownUser;
        }

        var hashedPwd = HashPwd(authPayload.Password, Convert.FromBase64String(user.Salt));

        if (!user.Pwd.Equals(hashedPwd)) {
            return EAuthResponse.WrongPassword;
        }

        var roles = UserRoleRepository.GetUserRolesByUserId(user.Id);

        var rolesArray = roles as Role[] ?? roles.ToArray();

        HttpContext.Session.SetInt32(UserIdKey, (int)user.Id);
        HttpContext.Session.SetString(UsernameKey, user.Username);
        HttpContext.Session.SetString(UserFullNameKey, user.FullName);
        HttpContext.Session.SetObject(UserRolesKey, rolesArray);
        HttpContext.Session.SetInt32(UserHasInitPwdKey, user.PwdInit ? 1 : 0);
        return EAuthResponse.Success;
    }

    [HttpGet]
    public EAuthResponse Logout() {
        var name = HttpContext.Session.GetString(UsernameKey);
        if (name == null) {
            return EAuthResponse.UnknownUser;
        }

        HttpContext.Session.Clear();

        return EAuthResponse.Success;
    }

    [HttpGet("info")]
    public LoggedUserPayload GetLoggedInUser() {
        //todo remove latter

        HttpContext.Session.SetInt32(UserIdKey, 1);
        HttpContext.Session.SetString(UsernameKey, "DummyOrion");
        HttpContext.Session.SetString(UserFullNameKey, "Dummy Full Name");
        HttpContext.Session.SetObject(UserRolesKey, DummyRoles);
        HttpContext.Session.SetInt32(UserHasInitPwdKey, 0);
        return new LoggedUserPayload("DummyOrion", "Dummy Full Name", DummyRoles, false);
        

        var name = HttpContext.Session.GetString(UsernameKey);
        if (name == null) {
            return new LoggedUserPayload(null, null, null, false);
        }

        return new LoggedUserPayload(name, HttpContext.Session.GetString(UserFullNameKey),
            HttpContext.Session.GetObject<Role[]>(UserRolesKey), HttpContext.Session.GetInt32(UserHasInitPwdKey) != 0);
    }


    [HttpPatch("changePwd")]
    public void LoggedInUserChangePwd([FromBody] string newPwd) {
        var userId = HttpContext.Session.GetInt32(UserIdKey);
        if (userId == null) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        var user = UserRepository.GetUserById((long)userId);
        if (user == null) {
            HttpContext.Session.Clear();
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }

        if (!UserUtils.IsValidPassword(newPwd)) {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            return;
        }


        user.Pwd = HashPwd(newPwd, Convert.FromBase64String(user.Salt));
        var ok = UserRepository.UpdateUserPwd(user);
        Response.StatusCode = (int)HttpStatusCode.OK;
    }


    public static string HashPwd(string password, byte[] salt) {
        // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
        var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            100000,
            256 / 8));
        return hashed;
    }


    public static Role[]? GetLoggedUserRoles(ISession session) => session.GetObject<Role[]>(UserRolesKey);

    public static bool IsUserAuthorized(ERoleType? neededRole, ISession session) {
        var roles = GetLoggedUserRoles(session);

        return IsUserAuthorized(neededRole, roles);
    }

    public static bool IsUserAuthorized(ERoleType? neededRole, Role[]? roles) {
        var userAuthorized = roles != null && (neededRole == null ||
                                               //no one is logged in
                                               roles.Any(role => role.Type.Equals(neededRole)));
        return userAuthorized;
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

    public static long? GetLoggedUserId(ISession session) => session.GetInt32(UserIdKey);
}