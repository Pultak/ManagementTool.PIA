using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Users;

public interface IUserDataService {

    public IEnumerable<User> GetAllUsers();
    public long AddUser(User user);
    public void DeleteUser(long id);

    public User? GetUserById(long id);
    public User? GetUserByName(string username);
    public void UpdateUser(User user);

    public IEnumerable<User> GetAllUsersByRole(Role role);

    public bool AssignUserToProject(User user, Project project);
    public bool IsUserAssignedToProject(User user, Project project);
}