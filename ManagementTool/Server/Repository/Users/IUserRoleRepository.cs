using ManagementTool.Server.Models.Business;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Repository.Users;

public interface IUserRoleRepository {

    /// <summary>
    /// Data source returns all roles that are not of project manager type
    /// </summary>
    /// <returns></returns>
    public IEnumerable<RoleBLL> GetAllNonProjectManRoles();
    /// <summary>
    /// Data source returns all user roles that are assigned to user
    /// </summary>
    /// <param name="userId">user that holds the roles</param>
    /// <returns></returns>
    public IEnumerable<RoleBLL> GetUserRolesByUserId(long userId);

    /// <summary>
    /// Returns all roles that are of specified type
    /// </summary>
    /// <param name="type">specified role type</param>
    /// <returns></returns>
    public IEnumerable<RoleBLL> GetRolesByType(RoleType type);
    
    /// <summary>
    /// Creates new role inside of data source
    /// </summary>
    /// <param name="role">New role object</param>
    /// <returns>id of new role, -1 on failure</returns>
    public long AddRole(RoleBLL role);

    /// <summary>
    /// Updates the project manager role name 
    /// </summary>
    /// <param name="projectId">id of project and project manager role</param>
    /// <param name="roleName">new name</param>
    /// <returns>true on success</returns>
    public bool UpdateProjectRoleName(long projectId, string roleName);

    /// <summary>
    /// Deletes the project manager role (used on project deletion)
    /// </summary>
    /// <param name="projectId">id of project and project manager role</param>
    /// <returns>true on success</returns>
    public bool DeleteProjectRole(long projectId);

    /// <summary>
    /// Method assigns all roles to specified user
    /// </summary>
    /// <param name="roles">ids of all existing roles </param>
    /// <param name="userId">user that will have new roles assigned</param>
    /// <returns>true on success</returns>
    public bool AssignRolesToUser(List<long> roles, long userId);

    /// <summary>
    /// Method changes the project manager user of specified project
    /// </summary>
    /// <param name="projectId"> project that should have new project manager </param>
    /// <param name="userId">new project manager</param>
    /// <returns>true on success</returns>
    public bool UpdateProjectManager(long projectId, long userId);

    /// <summary>
    /// Method removes all roles from specified user
    /// </summary>
    /// <param name="roles">ids of all existing roles </param>
    /// <param name="userId">user that will have new roles removed</param>
    /// <returns>true on success</returns>

    public bool UnassignRolesFromUser(List<long> roles, long userId);

    /// <summary>
    /// Gets the project manager role by project id
    /// </summary>
    /// <param name="projectId">id of project you want role of</param>
    /// <returns>null if there is no such project role</returns>
    public RoleBLL? GetRoleByProjectId(long projectId);
}