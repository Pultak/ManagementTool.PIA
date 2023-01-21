using System.Net;
using ManagementTool.Server.Services.Roles;
using ManagementTool.Shared.Models.Api.Payloads;
using ManagementTool.Shared.Models.Api.Requests;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Server.Services.Users; 

public class UsersService : IUsersService {

    private IUserRepository UserRepository { get; }
    private IAuthService AuthService { get; }
    private IRolesService RolesService { get; }

    public UsersService(IUserRepository userRepository, IAuthService authService, IRolesService rolesService) {
        UserRepository = userRepository;
        AuthService = authService;
        RolesService = rolesService;
    }

    public IEnumerable<UserBase> GetUsers() {
        var result = UserRepository.GetAllUsers();
        return result;
    }


    public EUserCreationResponse CreateUser(User userRequest) {
        var valResult = UserUtils.ValidateUser(userRequest);
        if (valResult != EUserCreationResponse.Ok || !UserUtils.IsValidPassword(userRequest.Pwd)) {
            return valResult;
        }
        
        valResult = CheckUserDataConflicts(userRequest);
        if (valResult != EUserCreationResponse.Ok) {
            return valResult;
        }

        var salt = AuthService.GenerateSalt(); 

        userRequest.Pwd = AuthService.HashPwd(userRequest.Pwd, salt);
        userRequest.Salt = Convert.ToBase64String(salt);
        

        var userId = UserRepository.AddUser(userRequest);
        userRequest.Id = userId;

        if (userId < 0) {
            return EUserCreationResponse.UsernameTaken;
        }
        
        return EUserCreationResponse.Ok;
    }



    public IEnumerable<DataModelAssignment<UserBase>>? GetAllUsersUnderProject(long projectId) {
        if (projectId < 1) {
            return null;
        }

        var result = UserRepository.GetAllUsersAssignedToProject(projectId);
        return result;
    }
    

    public EUserCreationResponse UpdateUser(UserBase user) {
        var valResult = UserUtils.ValidateUser(user);
        if (valResult != EUserCreationResponse.Ok) {
            return valResult;
        }

        var dbUser = new User(user);
        UserRepository.UpdateUser(dbUser);
        return EUserCreationResponse.Ok;
    }

    public IEnumerable<UserBase> GetAllUsersWithRole(ERoleType roleType) {
        var role = RolesService.GetRoleByType(roleType);
        if (role != null) {
            return Array.Empty<UserBase>();
        }

        var result = UserRepository.GetAllUsersByRole(role);
        return result;
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
        var ok = UserRepository.DeleteUser(user);
        return ok;
    }


    private EUserCreationResponse CheckUserDataConflicts(User user) {
        var allUsers = UserRepository.GetAllUsers();
        if (allUsers.Any(existingUser => existingUser.Username.Equals(user.Username))) {
            return EUserCreationResponse.UsernameTaken;
        }
        return EUserCreationResponse.Ok;
    }
    
    
    
    public void UpdateUserSuperiorAssignments(IReadOnlyCollection<UserBase> newAssignedSuperiors, UserBase user) {
        var dbAssignedSuperiors = UserRepository.GetAllUserSuperiorsIds(user.Id).ToArray();

        //select all superior references that should be removed from db
        //if not present in newList then remove from db
        var unassignList = dbAssignedSuperiors.Where(x => newAssignedSuperiors.All(newAssign => newAssign.Id != x)).ToList();
        
        //select all superior references that should be added to the db
        //if not present in dbList then add to db
        var assignList = newAssignedSuperiors.Where(x => !dbAssignedSuperiors.Contains(x.Id)).Select(x=> x.Id).ToList();
        
        if (assignList.Count > 0) {
            UserRepository.AssignSuperiorsToUser(assignList, user);
        }

        if (unassignList.Count > 0) {
            UserRepository.UnassignSuperiorsFromUser(unassignList, user);
        }
    }




}