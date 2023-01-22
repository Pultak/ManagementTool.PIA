using ManagementTool.Server.Models.Business;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Repository.Users;

public interface IUserRoleRepository {

    public IEnumerable<RoleBLL> GetAllRoles();
    public IEnumerable<RoleBLL> GetAllNonProjectManRoles();
    public IEnumerable<RoleBLL> GetUserRolesByUserId(long userId);
    public IEnumerable<RoleBLL> GetRolesByType(ERoleType type);
    public long AddRole(RoleBLL role);
    public bool UpdateProjectRoleName(long projectId, string roleName);
    public bool DeleteProjectRole(long projectId);

    public bool AssignRolesToUser(List<long> roles, long userId);
    public bool UnassignRolesFromUser(List<long> roles, long userId);

}