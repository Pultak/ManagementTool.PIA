using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Server.Services.Roles;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Server.Services.Users;

public class UsersService : IUsersService {
    public UsersService(IUserRepository userRepository, IAuthService authService,
        IRolesService rolesService, IMapper mapper) {
        UserRepository = userRepository;
        AuthService = authService;
        RolesService = rolesService;
        Mapper = mapper;
    }

    private IUserRepository UserRepository { get; }
    private IAuthService AuthService { get; }
    private IRolesService RolesService { get; }
    private IMapper Mapper { get; }

    /// <summary>
    /// Gets all users from data source
    /// </summary>
    /// <returns>All users from data source</returns>
    public IEnumerable<UserBasePL> GetUsers() {
        var result = UserRepository.GetAllUsers();
        return Mapper.Map<IEnumerable<UserBasePL>>(result);
    }


    /// <summary>
    /// Method for creation of new user. It also contains validation and hashing of pwd
    /// </summary>
    /// <param name="user">new user you want to create</param>
    /// <param name="pwd">password for new user</param>
    /// <returns>enum of all possible creation states + new user id on success</returns>
    public (UserCreationResponse response, long newId) CreateUser(UserBasePL newUser, string pwd) {
        var valResult = UserUtils.ValidateUser(newUser);
        var blNewUser = Mapper.Map<UserBaseBLL>(newUser);

        if (valResult != UserCreationResponse.Ok
            //|| !UserUtils.IsValidPassword(pwd)  password of the new user is generated
            ) {
            return (valResult, default);
        }

        valResult = CheckUserDataConflicts(blNewUser);
        if (valResult != UserCreationResponse.Ok) {
            return (valResult, default);
        }

        var salt = AuthService.GenerateSalt();

        var hashedPwd = AuthService.HashPwd(pwd, salt);


        var userId = UserRepository.AddUser(blNewUser, hashedPwd, Convert.ToBase64String(salt));
        if (userId < 1) {
            return (UserCreationResponse.EmptyUser, default);
        }

        return (UserCreationResponse.Ok, userId);
    }


    /// <summary>
    /// Returns all users from project that are assigned to project
    /// </summary>
    /// <param name="projectId">id of the project for desired users</param>
    /// <returns>null if invalid project, project users otherwise</returns>
    public IEnumerable<DataModelAssignmentPL<UserBasePL>>? GetAllUsersUnderProject(long projectId) {
        if (projectId < 1) {
            return null;
        }

        var result = UserRepository.GetAllUsersAssignedToProject(projectId);
        var plResult = result.Select(x => x.MapToPL<UserBasePL>(Mapper));

        return plResult;
    }


    /// <summary>
    /// Method for updating of existing user. It checks his presence in data source and validates the new data
    /// </summary>
    /// <param name="dbUser">User you want to update</param>
    /// <returns>enum of all possible creation states + new user id on success</returns>
    public UserCreationResponse UpdateUser(UserBasePL user) {
        var valResult = UserUtils.ValidateUser(user);
        if (valResult != UserCreationResponse.Ok) {
            return valResult;
        }

        var blUserBase = Mapper.Map<UserBaseBLL>(user);

        UserRepository.UpdateUser(blUserBase);
        return UserCreationResponse.Ok;
    }

    /// <summary>
    /// Method for getting all users with desired role
    /// </summary>
    /// <param name="roleType">type of the user you want to get</param>
    /// <returns>user list of all users with correct role</returns>
    public IEnumerable<UserBasePL> GetAllUsersWithRole(RoleType roleType) {
        if (roleType == RoleType.NoRole) {
            return Array.Empty<UserBasePL>();
        }

        var role = RolesService.GetRoleByType(roleType);
        if (role == null) {
            return Array.Empty<UserBasePL>();
        }

        var result = UserRepository.GetAllUsersByRole(role.Id);
        return Mapper.Map<IEnumerable<UserBasePL>>(result);
    }


    /// <summary>
    /// Method for receiving all users superiors ids. 
    /// </summary>
    /// <param name="userId">user of which you want to get superiors from</param>
    /// <returns>all superior ids from desired user</returns>
    public IEnumerable<long>? GetAllUserSuperiorsIds(long userId) {
        if (userId < 1) {
            return null;
        }

        var supIds = UserRepository.GetAllUserSuperiorsIds(userId);
        return supIds;
    }


    /// <summary>
    /// Method for deletion of user. The user must be present in data source
    /// </summary>
    /// <param name="userId">Id of user you want to delete</param>
    /// <returns>true on success, false otherwise</returns>
    public bool DeleteUser(long userId) {
        if (userId < 1) {
            return false;
        }

        var user = UserRepository.GetUserById(userId);

        if (user == null) {
            return false;
        }

        var ok = UserRepository.DeleteUser(user.Id);
        return ok;
    }

    /// <summary>
    /// Updates all user superiors in the data sources.
    /// Then list of superior is split and compared by existing superior assignments in the data source
    /// </summary>
    /// <param name="newAssignedSuperiors">list of all assigned superiors</param>
    /// <param name="user">user you want to update superiors of</param>
    public void UpdateUserSuperiorAssignments(IEnumerable<UserBasePL> newAssignedSuperiors, UserBasePL user) {
        var dbAssignedSuperiors = UserRepository.GetAllUserSuperiorsIds(user.Id).ToArray();

        //select all superior references that should be removed from db
        //if not present in newList then remove from db
        var unassignList = dbAssignedSuperiors.Where(x => newAssignedSuperiors.All(newAssign => newAssign.Id != x))
            .ToList();

        //select all superior references that should be added to the db
        //if not present in dbList then add to db
        var assignList = newAssignedSuperiors.Where(x => !dbAssignedSuperiors.Contains(x.Id)).Select(x => x.Id)
            .ToList();

        if (assignList.Count > 0) {
            UserRepository.AssignSuperiorsToUser(assignList, user.Id);
        }

        if (unassignList.Count > 0) {
            UserRepository.UnassignSuperiorsFromUser(unassignList, user.Id);
        }
    }


    private UserCreationResponse CheckUserDataConflicts(UserBaseBLL user) {
        var dbUser = UserRepository.GetUserByName(user.Username);
        if (dbUser != null) {
            return UserCreationResponse.UsernameTaken;
        }

        return UserCreationResponse.Ok;
    }
}