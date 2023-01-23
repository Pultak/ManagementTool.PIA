using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Utils;
using System.Net;
using ManagementTool.Server.Models.Business;

namespace ManagementTool.Server.Services.Users;

public interface IAuthService {
    public const string UserIdKey = "_Id";
    public const string UsernameKey = "_Username";
    public const string UserFullNameKey = "_FullName";
    public const string UserRolesKey = "_Roles";
    public const string UserHasInitPwdKey = "_InitPwd";

    public (AuthResponse authResponse, HttpStatusCode statusCode) Login(AuthRequest authRequest);

    public AuthResponse Logout();


    public LoggedUserPayload GetLoggedInUser();
    public HttpStatusCode LoggedInUserChangePwd(string newPwd);

    public bool IsUserAuthorized(ERoleType? neededRole);

    public string HashPwd(string password, byte[] salt);

    public RoleBLL[]? GetLoggedUserRoles();


    public RoleBLL[]? GetAllProjectManagerRoles();

    public RoleBLL[] GetAllProjectManagerRoles(RoleBLL[] roles);
    public IEnumerable<long> GetAllProjectManagerProjectIds();
    public IEnumerable<long> GetAllProjectManagerProjectIds(RoleBLL[] roles);

    public long? GetLoggedUserId();


    public bool IsAuthorizedToManageAssignments(long relevantProjectId);

    public bool IsAuthorizedToManageProjects(long projectId);

    public bool IsAuthorizedToViewUsers();

    public byte[] GenerateSalt();

}