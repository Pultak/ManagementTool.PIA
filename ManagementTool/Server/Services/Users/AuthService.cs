using System.Net;
using System.Security.Cryptography;
using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace ManagementTool.Server.Services.Users;

public class AuthService : IAuthService {

    private IHttpContextAccessor Accessor { get; }
    private IUserRepository UserRepository { get; }
    private IUserRoleRepository RoleRepository { get; }
    private IMapper Mapper { get; }
    public AuthService(IHttpContextAccessor accessor, IUserRepository userRepository, 
        IUserRoleRepository roleRepository, IMapper mapper) {
        Accessor = accessor;
        UserRepository = userRepository;
        RoleRepository = roleRepository;
        Mapper = mapper;
    }

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
        Accessor.HttpContext?.Session.SetInt32(IAuthService.UserIdKey, (int)user.Id);
        Accessor.HttpContext?.Session.SetString(IAuthService.UsernameKey, user.Username);
        Accessor.HttpContext?.Session.SetString(IAuthService.UserFullNameKey, user.FullName);
        Accessor.HttpContext?.Session.SetObject(IAuthService.UserRolesKey, rolesArray);
        Accessor.HttpContext?.Session.SetInt32(IAuthService.UserHasInitPwdKey, user.PwdInit ? 1 : 0);
        return (AuthResponse.Success, HttpStatusCode.OK);
    }


     public AuthResponse Logout() {
        var name = Accessor.HttpContext?.Session.GetString(IAuthService.UsernameKey);
        if (name == null) {
            return AuthResponse.UnknownUser;
        }

        Accessor.HttpContext?.Session.Clear();

        return AuthResponse.Success;
     }

     public LoggedUserPayload GetLoggedInUser(){
         var name = Accessor.HttpContext?.Session.GetString(IAuthService.UsernameKey);
         if (name == null) {
             return new LoggedUserPayload();
         }

         var blRoles = Accessor.HttpContext?.Session.GetObject<RoleBLL[]>(IAuthService.UserRolesKey);
         var result = new LoggedUserPayload(name, 
             Accessor.HttpContext?.Session.GetString(IAuthService.UserFullNameKey),
             Mapper.Map<RolePL[]>(blRoles),
             Accessor.HttpContext?.Session.GetInt32(IAuthService.UserHasInitPwdKey) != 0);

         return result;

     }

    public HttpStatusCode LoggedInUserChangePwd(string newPwd) {
        var userId = Accessor.HttpContext?.Session.GetInt32(IAuthService.UserIdKey);
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
        Accessor.HttpContext?.Session.SetInt32(IAuthService.UserHasInitPwdKey, 0); // false
        UserRepository.UpdateUserPwd(user.Value.id, hashedPwd);
        return HttpStatusCode.OK;
    }


    public bool IsUserAuthorized(ERoleType? neededRole) {
        if (Accessor.HttpContext?.Session == null) {
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


    public RoleBLL[]? GetLoggedUserRoles() => Accessor.HttpContext?.Session.GetObject<RoleBLL[]>(IAuthService.UserRolesKey);


    public bool IsUserAuthorized(ERoleType? neededRole, RoleBLL[]? roles) {
        var userAuthorized = roles != null && (neededRole == null ||
                                               //no one is logged in
                                               roles.Any(role => role.Type.Equals(neededRole)));
        return userAuthorized;
    }

    public RoleBLL[]? GetAllProjectManagerRoles() {
        var roles = GetLoggedUserRoles();
        if (roles == null) {
            return null;
        }

        return GetAllProjectManagerRoles(roles);
    }

    public RoleBLL[] GetAllProjectManagerRoles(RoleBLL[] roles) {
        return roles.Where(role => role.Type == ERoleType.ProjectManager).ToArray();
    }
    public IEnumerable<long> GetAllProjectManagerProjectIds() {
        var roles = GetAllProjectManagerRoles();
        if (roles == null) {
            return Enumerable.Empty<long>();
        }
        var manRoles = GetAllProjectManagerRoles(roles);
        var resultIds = manRoles.Select(x => x.ProjectId).OfType<long>();
        return resultIds;
    }
    public IEnumerable<long> GetAllProjectManagerProjectIds(RoleBLL[] roles) {
        var manRoles = GetAllProjectManagerRoles(roles);
        var resultIds = manRoles.Select(x => x.ProjectId).OfType<long>();
        return resultIds;
    }

    public long? GetLoggedUserId() => Accessor.HttpContext?.Session.GetInt32(IAuthService.UserIdKey);

    
    public bool IsAuthorizedToManageAssignments(long relevantProjectId) {
        var userRoles = GetLoggedUserRoles();
        if (userRoles == null) {
            return false;
        }
        if (IsUserAuthorized(ERoleType.DepartmentManager, userRoles)) {
            //all ok, department manager can do everything
            return true;
        }

        if (!IsUserAuthorized(ERoleType.ProjectManager, userRoles)) {
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
        if (IsUserAuthorized(ERoleType.DepartmentManager, userRoles) ||
            IsUserAuthorized(ERoleType.Secretariat, userRoles)) {
            //all ok, department manager and secretariat can do everything with projects
            return true;
        }

        if (!IsUserAuthorized(ERoleType.ProjectManager, userRoles)) {
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
        if (IsUserAuthorized(ERoleType.Secretariat, userRoles) ||
            IsUserAuthorized(ERoleType.ProjectManager, userRoles) ||
            IsUserAuthorized(ERoleType.DepartmentManager, userRoles)) {
            //all ok, department manager and secretariat can do everything with projects
            return true;
        }

        return false;
    }


    public byte[] GenerateSalt(){
        // Generate a 128-bit salt using a sequence of
        // cryptographically strong random bytes.
        return RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes
    }

}