using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace ManagementTool.Server.Services.Users;

public class AuthService : IAuthService {
    public AuthService(IHttpContextAccessor accessor, IUserRepository userRepository,
        IUserRoleRepository roleRepository, IMapper mapper, IConfiguration config) {
        Accessor = accessor;
        UserRepository = userRepository;
        RoleRepository = roleRepository;
        Mapper = mapper;

        var securitySection = config.GetSection("Security");
        var key = securitySection["JWTTokenKey"];
        TokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
        var ok = int.TryParse(securitySection["JWTTimeoutDays"], out var result);
        JWTTokenTimeout = ok ? result : 1;
    }

    private IHttpContextAccessor Accessor { get; }
    private IUserRepository UserRepository { get; }
    private IUserRoleRepository RoleRepository { get; }
    private IMapper Mapper { get; }

    private SymmetricSecurityKey TokenKey { get; }
    private int JWTTokenTimeout { get; }


    /// <summary>
    /// Method for checking if the auth request has existing user,
    /// valid password and saves the user data to the session
    /// </summary>
    /// <param name="authRequest"></param>
    /// <returns>auth validation enum and http status code for this state</returns>
    public (AuthResponse authResponse, HttpStatusCode statusCode) Login(AuthRequest authRequest) {
        if (string.IsNullOrEmpty(authRequest.Password) || string.IsNullOrEmpty(authRequest.Username)) {
            return (AuthResponse.BadRequest, HttpStatusCode.BadRequest);
        }

        if (IsUserAuthorized(null)) {
            //already logged in
            return (AuthResponse.AlreadyLoggedIn, HttpStatusCode.OK);
        }

        var userCredentials = UserRepository.GetUserCredentials(authRequest.Username);
        if (userCredentials == null) {
            return (AuthResponse.UnknownUser, HttpStatusCode.Unauthorized);
        }

        var hashedPwd = HashPwd(authRequest.Password, Convert.FromBase64String(userCredentials.Value.salt));

        if (!userCredentials.Value.pwd.Equals(hashedPwd)) {
            return (AuthResponse.WrongPassword, HttpStatusCode.Unauthorized);
        }

        var user = UserRepository.GetUserById(userCredentials.Value.id);
        if (user == null) {
            //user not found in db. Should not happen
            return (AuthResponse.UnknownUser, HttpStatusCode.Unauthorized);
        }

        var roles = RoleRepository.GetUserRolesByUserId(user.Id);

        var rolesArray = roles as RoleBLL[] ?? roles.ToArray();
        var usrInfo = new SessionInfo { User = user, Roles = rolesArray };
        GenerateJwtToken(usrInfo);
        //FillTheSession(usrInfo, token);
        return (AuthResponse.Success, HttpStatusCode.OK);
    }

    /*
    private string GenerateToken(SessionInfo userInfo) {
        var token = UserUtils.CreateRandomString(100);
        Tokens.UserMap.TryAdd(token, userInfo);
        return token;
    }*/


    /*
    /// <summary>
    /// Checks the passed token and restores the session if it is present 
    /// </summary>
    /// <param name="token">token received from the client</param>
    /// <returns>auth validation enum and http status code for this state</returns>
    public (AuthResponse authResponse, HttpStatusCode statusCode) RenewSessionFromToken(string token) {
        if (IsUserAuthorized(null)) {
            //already logged in
            return (AuthResponse.AlreadyLoggedIn, HttpStatusCode.OK);
        }
        var tokenExists = Tokens.UserMap.ContainsKey(token);
        if (!tokenExists) {
            return (AuthResponse.UnknownUser, HttpStatusCode.Unauthorized);
        }
        FillTheSession(Tokens.UserMap[token], token);
        
        return (AuthResponse.Success, HttpStatusCode.OK);
    }
    */


    private void GenerateJwtToken(SessionInfo userInfo) {
        var claims = new[] {
            new Claim(ClaimTypes.Name, userInfo.User.Username),
            new Claim(ClaimTypes.Role, RoleType.NoRole.ToString()),
            new Claim(IAuthService.UserIdKey, userInfo.User.Id.ToString()),
            new Claim(IAuthService.UsernameKey, userInfo.User.Username),
            new Claim(IAuthService.UserFullNameKey, userInfo.User.FullName),
            new Claim(IAuthService.UserRolesKey, JsonConvert.SerializeObject(userInfo.Roles)),
            new Claim(IAuthService.UserHasInitPwdKey, userInfo.User.PwdInit ? "1" : "0"),
        };



        var credentials = new SigningCredentials(TokenKey, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(JWTTokenTimeout),
            signingCredentials: credentials
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        Accessor.HttpContext?.Session.SetString(IAuthService.UserJWTToken, jwt);
    }



    /// <summary>
    /// Logs out the user by clearing session info 
    /// </summary>
    /// <returns>AuthResponse ok on success or Unknown user if no user is logged in</returns>
    public AuthResponse Logout() {
        /*var name = Accessor.HttpContext?.Session.GetString(IAuthService.UsernameKey);
        if (name == null) {
            return AuthResponse.UnknownUser;
        }*/

        Accessor.HttpContext?.Session.Clear();

        return AuthResponse.Success;
    }

    /// <summary>
    /// Gets information about current logged in user. If there is no user the values in the object are empty
    /// </summary>
    /// <returns>logged in user with all his crucial data</returns>
    public LoggedUserPayload GetLoggedInUser() {
        var name = Accessor.HttpContext?.User.FindFirstValue(IAuthService.UsernameKey);
        if (name == null) {
            return new LoggedUserPayload();
        }

        var blRolesString = Accessor.HttpContext?.User.FindFirstValue(IAuthService.UserRolesKey);
        if (blRolesString == null) {
            return new LoggedUserPayload();
        }
        var blRoles = JsonConvert.DeserializeObject<RoleBLL[]>(blRolesString);
        
        var result = new LoggedUserPayload(name,
            Accessor.HttpContext?.User.FindFirstValue(IAuthService.UserFullNameKey),
            Mapper.Map<RolePL[]>(blRoles),
            Accessor.HttpContext?.User.FindFirstValue(IAuthService.UserHasInitPwdKey).Equals("1") ?? false);

        return result;
    }

    /// <summary>
    /// Method that changes password of the logged user 
    /// </summary>
    /// <param name="newPwd">new password for the user</param>
    /// <returns></returns>
    public HttpStatusCode LoggedInUserChangePwd(string newPwd) {
        var userId = GetLoggedUserId();
        if (userId == null) {
            return HttpStatusCode.Unauthorized;
        }


        var user = UserRepository.GetUserCredentials((long)userId);
        if (user == null) {
            Accessor.HttpContext?.Session.Clear();
            return HttpStatusCode.NotFound;
        }

        if (!UserUtils.IsValidPassword(newPwd)) {
            return HttpStatusCode.UnprocessableEntity;
        }


        var hashedPwd = HashPwd(newPwd, Convert.FromBase64String(user.Value.salt));
        //Accessor.HttpContext?.Session.SetInt32(IAuthService.UserHasInitPwdKey, 0); // false
        UserRepository.UpdateUserPwd(user.Value.id, hashedPwd);
        return HttpStatusCode.OK;
    }

    
    public bool IsUserAuthorized(RoleType? neededRole) {
        var userName = Accessor.HttpContext?.User.Identity?.Name;
        if (userName == null) {
            return false;
        }

        var roles = GetLoggedUserRoles();

        return IsUserAuthorized(neededRole, roles);
    }


    public string HashPwd(string password, byte[] salt) {
        // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
        var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            100000,
            256 / 8));
        return hashed;
    }


    /// <summary>
    /// Gets all roles of the current logged in user 
    /// </summary>
    /// <returns>all roles current logged in user posses with</returns>
    public RoleBLL[]? GetLoggedUserRoles() {
        var rolesString = Accessor.HttpContext?.User.FindFirstValue(IAuthService.UserRolesKey);
        if (rolesString == null) {
            return null;
        }

        var roles = JsonConvert.DeserializeObject<RoleBLL[]>(rolesString);

        return roles;
    }

    /// <summary>
    /// Gets all manager roles of the current logged in user 
    /// </summary>
    /// <returns>all roles current logged in user posses with</returns>
    public RoleBLL[]? GetAllProjectManagerRoles() {
        var roles = GetLoggedUserRoles();
        if (roles == null) {
            return null;
        }

        return GetAllProjectManagerRoles(roles);
    }

    public RoleBLL[] GetAllProjectManagerRoles(RoleBLL[] roles) {
        return roles.Where(role => role.Type == RoleType.ProjectManager).ToArray();
    }

    /// <summary>
    /// Gets all project ids the logged in user can access
    /// </summary>
    /// <returns>list of all accessible project ids</returns>
    public IEnumerable<long> GetAllProjectManagerProjectIds() {
        var roles = GetAllProjectManagerRoles();
        if (roles == null) {
            return Enumerable.Empty<long>();
        }

        var manRoles = GetAllProjectManagerRoles(roles);
        var resultIds = manRoles.Select(x => x.ProjectId).OfType<long>();
        return resultIds;
    }

    /// <summary>
    /// Gets all project ids the logged in user can access
    /// </summary>
    /// <returns>list of all accessible project ids</returns>
    public IEnumerable<long> GetAllProjectManagerProjectIds(RoleBLL[] roles) {
        var manRoles = GetAllProjectManagerRoles(roles);
        var resultIds = manRoles.Select(x => x.ProjectId).OfType<long>();
        return resultIds;
    }

    /// <summary>
    /// Get the id of the logged in user stored in session
    /// </summary>
    /// <returns>id of the logged in user, null if not user is logged in</returns>
    //public long? GetLoggedUserId() => Accessor.HttpContext?.Session.GetInt32(IAuthService.UserIdKey);
    public long? GetLoggedUserId() {
        var userId = Accessor.HttpContext?.User.FindFirstValue(IAuthService.UserIdKey);
        if (userId == null) {
            return null;
        }
        var ok = long.TryParse(userId, out var output);
        return ok? output : null;
    }


    public bool IsAuthorizedToManageAssignments(long relevantProjectId) {
        var userRoles = GetLoggedUserRoles();
        if (userRoles == null) {
            return false;
        }

        if (IsUserAuthorized(RoleType.DepartmentManager, userRoles)) {
            //all ok, department manager can do everything
            return true;
        }

        if (!IsUserAuthorized(RoleType.ProjectManager, userRoles)) {
            return false;
        }

        var managerRoles = GetAllProjectManagerRoles(userRoles);

        //can this manager manage assignments for this project?
        var result = managerRoles.Any(x => x.ProjectId == relevantProjectId);

        return result;
    }


    public bool IsAuthorizedToManageProjects(long projectId) {
        var userRoles = GetLoggedUserRoles();
        if (userRoles == null) {
            return false;
        }

        if (IsUserAuthorized(RoleType.DepartmentManager, userRoles) ||
            IsUserAuthorized(RoleType.Secretariat, userRoles)) {
            //all ok, department manager and secretariat can do everything with projects
            return true;
        }

        if (!IsUserAuthorized(RoleType.ProjectManager, userRoles)) {
            return false;
        }

        var managerRoles = GetAllProjectManagerRoles(userRoles);

        //can this manager manage assignments for this project?
        return managerRoles.Any(x => x.ProjectId == projectId);
    }

    public bool IsAuthorizedToViewUsers() {
        var userRoles = GetLoggedUserRoles();
        if (userRoles == null) {
            return false;
        }

        if (IsUserAuthorized(RoleType.Secretariat, userRoles) ||
            IsUserAuthorized(RoleType.ProjectManager, userRoles) ||
            IsUserAuthorized(RoleType.DepartmentManager, userRoles)) {
            //all ok, department manager and secretariat can do everything with projects
            return true;
        }

        return false;
    }


    public byte[] GenerateSalt() =>
        RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes


    public bool IsUserAuthorized(RoleType? neededRole, RoleBLL[]? roles) {
        var userAuthorized = roles != null && (neededRole == null ||
                                               //no one is logged in
                                               roles.Any(role => role.Type.Equals(neededRole)));
        return userAuthorized;
    }



    /*private void FillTheSession(SessionInfo userInfo, string token) {
        Accessor.HttpContext?.Session.SetInt32(IAuthService.UserIdKey, (int)userInfo.User.Id);
        Accessor.HttpContext?.Session.SetString(IAuthService.UsernameKey, userInfo.User.Username);
        Accessor.HttpContext?.Session.SetString(IAuthService.UserFullNameKey, userInfo.User.FullName);
        Accessor.HttpContext?.Session.SetObject(IAuthService.UserRolesKey, userInfo.Roles);
        Accessor.HttpContext?.Session.SetInt32(IAuthService.UserHasInitPwdKey, userInfo.User.PwdInit ? 1 : 0);
        Accessor.HttpContext?.Session.SetString(IAuthService.UserJWTToken, token);
    }*/

}