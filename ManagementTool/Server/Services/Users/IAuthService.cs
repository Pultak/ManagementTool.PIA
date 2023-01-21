using ManagementTool.Server.Controllers;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Utils;
using System.Net;
using System.Security.Cryptography;

namespace ManagementTool.Server.Services.Users;

public interface IAuthService {
    public const string UserIdKey = "_Id";
    public const string UsernameKey = "_Username";
    public const string UserFullNameKey = "_FullName";
    public const string UserRolesKey = "_Roles";
    public const string UserHasInitPwdKey = "_InitPwd";

    public (AuthResponse authResponse, HttpStatusCode statusCode) Login(AuthPayload authPayload);

    public AuthResponse Logout();


    public LoggedUserPayload? GetLoggedInUser();
    public HttpStatusCode LoggedInUserChangePwd(string newPwd);

    public bool IsUserAuthorized(ERoleType? neededRole);
    public bool IsUserAuthorized(ERoleType? neededRole, Role[]? roles);
    //todo public bool IsUserAuthorized(ERoleType[] possibleRoles);

    public string HashPwd(string password, byte[] salt);

    public Role[]? GetLoggedUserRoles();


    public Role[]? GetAllProjectManagerRoles();

    public Role[] GetAllProjectManagerRoles(Role[] roles);
    public IEnumerable<long> GetAllProjectManagerProjectIds();
    public IEnumerable<long> GetAllProjectManagerProjectIds(Role[] roles);

    public long? GetLoggedUserId();


    public bool IsAuthorizedToManageAssignments(long relevantProjectId);

    public bool IsAuthorizedToManageProjects(long projectId);

    public bool IsAuthorizedToViewUsers();

    public byte[] GenerateSalt();

}