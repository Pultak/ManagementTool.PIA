using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Users;

public interface IUserDataService {

    public IEnumerable<User> GetAllUsers();
    public long AddUser(User user);
    public bool DeleteUser(long id);
    public bool DeleteUser(User user);

    public User? GetUserById(long id);
    public User? GetUserByName(string username);
    public bool UpdateUser(User user);

    public IEnumerable<User> GetAllUsersByRole(Role role);

    public bool AssignUserToProject(User user, Project project);
    public bool UnassignUserFromProject(User user, Project project);
    public bool IsUserAssignedToProject(User user, Project project);
}