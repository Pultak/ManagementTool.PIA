using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Users;

public interface IUserRoleDataService {

    public IEnumerable<Role> GetAllRoles();
    public IEnumerable<Role> GetUserRolesById(long userId);
    public int AddRole(Role role);
    /* todo not needed + not recommended
    public void DeleteRole(long id);
    public void UpdateUser(User user);
    */
}