using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Users;

public interface IUserRoleDataService {

    public IEnumerable<Role> GetAllRoles();
    public IEnumerable<Role> GetUserRolesById(long userId);
    public long AddRole(Role role);
    public bool DeleteProjectRole(long projectId);
}