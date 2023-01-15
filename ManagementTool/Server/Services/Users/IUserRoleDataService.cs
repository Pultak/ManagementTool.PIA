using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Users;

public interface IUserRoleDataService {

    public IEnumerable<Role> GetAllRoles();
    public IEnumerable<Role> GetAllNonProjectManRoles();
    public IEnumerable<Role> GetUserRolesByUserId(long userId);
    public IEnumerable<Role> GetRolesByType(ERoleType type);
    public long AddRole(Role role);
    public bool UpdateProjectRoleName(long projectId, string roleName);
    public bool DeleteProjectRole(long projectId);

    public bool AssignRolesToUser(List<Role> roles, long userId);
    public bool UnassignRolesFromUser(List<Role> roles, long userId);

}