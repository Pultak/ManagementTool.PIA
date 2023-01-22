using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Server.Services.Roles;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Server.Services.Users; 

public class UsersService : IUsersService {

    private IUserRepository UserRepository { get; }
    private IAuthService AuthService { get; }
    private IRolesService RolesService { get; }
    private IMapper Mapper { get; }

    public UsersService(IUserRepository userRepository, IAuthService authService, IRolesService rolesService, IMapper mapper) {
        UserRepository = userRepository;
        AuthService = authService;
        RolesService = rolesService;
        Mapper = mapper;
    }

    public IEnumerable<UserBasePL> GetUsers() {
        var result = UserRepository.GetAllUsers();
        return Mapper.Map<IEnumerable<UserBasePL>>(result);
    }


    public EUserCreationResponse CreateUser(UserBasePL newUser, string pwd) {
        var valResult = UserUtils.ValidateUser(newUser);
        var blNewUser = Mapper.Map<UserBaseBLL>(newUser);

        if (valResult != EUserCreationResponse.Ok || !UserUtils.IsValidPassword(pwd)) {
            return valResult;
        }
        
        valResult = CheckUserDataConflicts(blNewUser);
        if (valResult != EUserCreationResponse.Ok) {
            return valResult;
        }

        var salt = AuthService.GenerateSalt(); 

        var hashedPwd = AuthService.HashPwd(pwd, salt);
        

        var userId = UserRepository.AddUser(blNewUser, hashedPwd, Convert.ToBase64String(salt));
        if (userId < 1) {
            return EUserCreationResponse.EmptyUser;
        }
        
        return EUserCreationResponse.Ok;
    }



    public IEnumerable<DataModelAssignmentPL<UserBasePL>>? GetAllUsersUnderProject(long projectId) {
        if (projectId < 1) {
            return null;
        }

        var result = UserRepository.GetAllUsersAssignedToProject(projectId);
        return Mapper.Map<IEnumerable<DataModelAssignmentPL<UserBasePL>>>(result);
    }
    

    public EUserCreationResponse UpdateUser(UserBasePL user) {
        var valResult = UserUtils.ValidateUser(user);
        if (valResult != EUserCreationResponse.Ok) {
            return valResult;
        }
        var blUserBase = Mapper.Map<UserBaseBLL>(user);
        
        UserRepository.UpdateUser(blUserBase);
        return EUserCreationResponse.Ok;
    }

    public IEnumerable<UserBasePL> GetAllUsersWithRole(ERoleType roleType) {
        if (roleType == ERoleType.NoRole) {
            return Array.Empty<UserBasePL>();
        }

        var role = RolesService.GetRoleByType(roleType);
        if (role == null) {
            return Array.Empty<UserBasePL>();
        }
        var result = UserRepository.GetAllUsersByRole(role.Id);
        return Mapper.Map<IEnumerable<UserBasePL>>(result);
    }

    public IEnumerable<long>? GetAllUserSuperiorsIds(long userId) {
        if (userId < 1) {
            return null;
        }

        var supIds = UserRepository.GetAllUserSuperiorsIds(userId);
        return supIds;
    }


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


    private EUserCreationResponse CheckUserDataConflicts(UserBaseBLL user) {
        var dbUser = UserRepository.GetUserByName(user.Username);
        if (dbUser != null) {
            return EUserCreationResponse.UsernameTaken;
        }
        return EUserCreationResponse.Ok;
    }
    
    
    
    public void UpdateUserSuperiorAssignments(IEnumerable<UserBasePL> newAssignedSuperiors, UserBasePL user) {
        var dbAssignedSuperiors = UserRepository.GetAllUserSuperiorsIds(user.Id).ToArray();

        //select all superior references that should be removed from db
        //if not present in newList then remove from db
        var unassignList = dbAssignedSuperiors.Where(x => newAssignedSuperiors.All(newAssign => newAssign.Id != x)).ToList();
        
        //select all superior references that should be added to the db
        //if not present in dbList then add to db
        var assignList = newAssignedSuperiors.Where(x => !dbAssignedSuperiors.Contains(x.Id)).Select(x=> x.Id).ToList();
        
        if (assignList.Count > 0) {
            UserRepository.AssignSuperiorsToUser(assignList, user.Id);
        }

        if (unassignList.Count > 0) {
            UserRepository.UnassignSuperiorsFromUser(unassignList, user.Id);
        }
    }




}