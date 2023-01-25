using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Roles;

public interface IRolesService {

    /// <summary>
    /// Creates new role inside the data source for newly created project
    /// The role normally goes like '<project-name> Manager'
    /// </summary>
    /// <param name="projectName">Name of the project you want a role for</param>
    /// <param name="projectId">Id of the project you want a role for</param>
    /// <returns>true on success</returns>
    public bool CreateNewProjectRole(string projectName, long projectId);

    /// <summary>
    /// Updates project role inside the data source for newly created project
    /// The role normally goes like '<project-name> Manager'
    /// </summary>
    /// <param name="projectName">Name of the project you want a role change to</param>
    /// <param name="projectId">Id of the project you want a role name update of</param>
    /// <returns>true on success</returns>
    public bool UpdateProjectRoleName(string projectName, long projectId);
    
    /// <summary>
    /// Updates the role by changing the project manager id 
    /// </summary>
    /// <param name="userId">New role manager you want to have</param>
    /// <param name="projectId">Project for which you want to have new manager</param>
    /// <returns>true on success</returns>
    public bool UpdateProjectManager(long userId, long projectId);

    /// <summary>
    /// Updates the roles of specified user. New roles can be assigned, or otherwise based on the input role list
    /// </summary>
    /// <param name="roleAssignments">list of all roles the user should have</param>
    /// <param name="userId">id of the user for which you want to update roles</param>
    public void UpdateUserRoleAssignments(List<DataModelAssignmentPL<RolePL>> roleAssignments, long userId);

    /// <summary>
    /// Returns all roles passed user have
    /// </summary>
    /// <param name="userId">user you have to collect role for</param>
    /// <returns>list of all assigned roles</returns>
    public IEnumerable<DataModelAssignmentPL<RolePL>>? GetAllRolesAssigned(long userId);

    /// <summary>
    /// Returns role object for specified role type
    /// </summary>
    /// <param name="roleType">Role type you want to collect</param>
    /// <returns>Role object from data source</returns>
    public RolePL? GetRoleByType(RoleType roleType);

    /// <summary>
    /// Method collect the user from data source by project manager role 
    /// </summary>
    /// <param name="projectId">id of the project for which a project manager should be obtained</param>
    /// <returns>Project manager, null if not existing</returns>
    public UserBasePL? GetManagerByProjectId(long projectId);
}