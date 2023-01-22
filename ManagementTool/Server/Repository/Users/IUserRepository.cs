using ManagementTool.Server.Models.Business;
namespace ManagementTool.Server.Repository.Users;

public interface IUserRepository {

    public IEnumerable<UserBaseBLL> GetAllUsers();

    public long AddUser(UserBaseBLL user, string pwd, string salt);
    public bool DeleteUser(long userId);

    public UserBaseBLL? GetUserById(long id);
    public IEnumerable<UserBaseBLL> GetUsersById(IEnumerable<long> userIds);
    public (long id, string pwd, string salt)? GetUserPassword(string username);
    public bool UpdateUser(UserBaseBLL user);
    public bool UpdateUserPwd(long userId, string newPwd);

    public IEnumerable<UserBaseBLL> GetAllUsersByRole(long roleId);
    public IEnumerable<long> GetAllUserSuperiorsIds(long userId);
    public IEnumerable<DataModelAssignmentBLL<UserBaseBLL>> GetAllUsersAssignedToProject(long projectId);
    public IEnumerable<UserBaseBLL> GetAllUsersUnderProject(long projectId);

    public bool AssignUsersToProject(List<long> usersIds, long projectId);
    public bool UnassignUsersFromProject(List<long> usersIds, long projectId);
    
    public bool AssignSuperiorsToUser(List<long> superiorsIds, long superiorId);
    public bool UnassignSuperiorsFromUser(List<long> superiorsIds, long superiorId);

    public bool IsUserAssignedToProject(long userId, long projectId);
}