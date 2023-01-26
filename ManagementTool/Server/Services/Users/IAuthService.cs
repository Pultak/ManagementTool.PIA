using System.Net;
using ManagementTool.Server.Models.Business;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Users;

public interface IAuthService {
    /// <summary>
    /// List of all keys for session data accessing
    /// </summary>
    public const string UserIdKey = "_Id";
    public const string UsernameKey = "_Username";
    public const string UserFullNameKey = "_FullName";
    public const string UserRolesKey = "_Roles";
    public const string UserHasInitPwdKey = "_InitPwd";
    public const string UserJWTToken = "_UserToken";

    /// <summary>
    /// Method for checking if the auth request has existing user,
    /// valid password and saves the user data to the session
    /// </summary>
    /// <param name="authRequest"></param>
    /// <returns>auth validation enum and http status code for this state</returns>
    public (AuthResponse authResponse, HttpStatusCode statusCode) Login(AuthRequest authRequest);

    /*todo remove
    /// <summary>
    /// Checks the passed token and restores the session if it is present 
    /// </summary>
    /// <param name="authRequest">token received from the client</param>
    /// <returns>auth validation enum and http status code for this state</returns>
    public (AuthResponse authResponse, HttpStatusCode statusCode) RenewSessionFromToken(string token);
    */
    /// <summary>
    /// Logs out the user by clearing session info 
    /// </summary>
    /// <returns>AuthResponse ok on success or Unknown user if no user is logged in</returns>
    public AuthResponse Logout();

    /// <summary>
    /// Gets information about current logged in user. If there is no user the values in the object are empty
    /// </summary>
    /// <returns>logged in user with all his crucial data</returns>
    public LoggedUserPayload GetLoggedInUser();

    /// <summary>
    /// Method that changes password of the logged user 
    /// </summary>
    /// <param name="newPwd">new password for the user</param>
    /// <returns></returns>
    public HttpStatusCode LoggedInUserChangePwd(string newPwd);

    /// <summary>
    /// Method checks the session data about user and validates if the user roles are enough
    /// </summary>
    /// <param name="neededRole">role you need to have</param>
    /// <returns>true if authorized, false otherwise</returns>
    public bool IsUserAuthorized(RoleType? neededRole);

    /// <summary>
    /// Hashes the password with a derive of 256-bit subkey (use HMACSHA256 with 100,000 iterations)
    /// </summary>
    /// <param name="password">password you want to hash</param>
    /// <param name="salt">generated salt to keep the passwords unique</param>
    /// <returns></returns>
    public string HashPwd(string password, byte[] salt);

    /// <summary>
    /// Gets all roles of the current logged in user 
    /// </summary>
    /// <returns>all roles current logged in user posses with</returns>
    public RoleBLL[]? GetLoggedUserRoles();


    /// <summary>
    /// Gets all manager roles of the current logged in user 
    /// </summary>
    /// <returns>all roles current logged in user posses with</returns>
    public RoleBLL[]? GetAllProjectManagerRoles();

    /// <summary>
    /// Gets all project ids the logged in user can access
    /// </summary>
    /// <returns>list of all accessible project ids</returns>
    public IEnumerable<long> GetAllProjectManagerProjectIds();
    /// <summary>
    /// Gets all project ids the logged in user can access
    /// </summary>
    /// <returns>list of all accessible project ids</returns>
    public IEnumerable<long> GetAllProjectManagerProjectIds(RoleBLL[] roles);

    /// <summary>
    /// Get the id of the logged in user stored in session
    /// </summary>
    /// <returns>id of the logged in user, null if not user is logged in</returns>
    public long? GetLoggedUserId();

    public bool IsAuthorizedToManageAssignments(long relevantProjectId);

    public bool IsAuthorizedToManageProjects(long projectId);

    public bool IsAuthorizedToViewUsers();

    /// <summary>
    /// 
    /// Generate a 128-bit salt using a sequence of
    /// cryptographically strong random bytes.
    /// </summary>
    /// <returns></returns>
    public byte[] GenerateSalt();
}