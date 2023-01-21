using ManagementTool.Shared.Models.Database;
using System.Collections.Generic;
using ManagementTool.Shared.Models.Api.Payloads;

namespace ManagementTool.Server.Services.Users;

public interface IUserRepository {

    public IEnumerable<UserBase> GetAllUsers();

    public long AddUser(User user);
    public bool DeleteUser(User user);

    public User? GetUserById(long id);
    public IEnumerable<UserBase> GetUsersById(IEnumerable<long> userIds);
    public User? GetUserByName(string username);
    public bool UpdateUser(User user);
    public bool UpdateUserPwd(User user);

    public IEnumerable<UserBase> GetAllUsersByRole(Role role);
    public IEnumerable<long> GetAllUserSuperiorsIds(long userId);
    public IEnumerable<DataModelAssignment<UserBase>> GetAllUsersAssignedToProject(long projectId);
    public IEnumerable<UserBase> GetAllUsersUnderProject(long projectId);

    public bool AssignUsersToProject(List<long> usersIds, Project project);
    public bool UnassignUsersFromProject(List<long> usersIds, Project project);
    
    public bool AssignSuperiorsToUser(List<long> superiorsIds, UserBase user);
    public bool UnassignSuperiorsFromUser(List<long> superiorsIds, UserBase user);

    public bool IsUserAssignedToProject(User user, Project project);
}