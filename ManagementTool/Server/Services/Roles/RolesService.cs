using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Roles;

public class RolesService : IRolesService {
    public RolesService(IUserRoleRepository rolesRepository, IMapper mapper, IUserRepository userRepository) {
        RolesRepository = rolesRepository;
        Mapper = mapper;
        UserRepository = userRepository;
    }

    private IUserRoleRepository RolesRepository { get; }
    private IUserRepository UserRepository { get; }
    private IMapper Mapper { get; }

    /// <summary>
    /// Creates new role inside the data source for newly created project
    /// The role normally goes like '<project-name> Manager'
    /// </summary>
    /// <param name="projectName">Name of the project you want a role for</param>
    /// <param name="projectId">Id of the project you want a role for</param>
    /// <returns>true on success</returns>
    public bool CreateNewProjectRole(string projectName, long projectId) {
        var newRole = new RoleBLL {
            Name = $"{projectName} Manažer",
            Type = RoleType.ProjectManager,
            ProjectId = projectId
        };
        var roleId = RolesRepository.AddRole(newRole);
        return roleId > 0;
    }

    /// <summary>
    /// Updates project role inside the data source for newly created project
    /// The role normally goes like '<project-name> Manager'
    /// </summary>
    /// <param name="projectName">Name of the project you want a role change to</param>
    /// <param name="projectId">Id of the project you want a role name update of</param>
    /// <returns>true on success</returns>
    public bool UpdateProjectRoleName(string newProjectName, long projectId) {
        if (projectId < 1 || string.IsNullOrEmpty(newProjectName)) {
            return false;
        }

        var result = RolesRepository.UpdateProjectRoleName(projectId, projectId + " Manažer");
        return result;
    }


    /// <summary>
    /// Updates the role by changing the project manager id 
    /// </summary>
    /// <param name="userId">New role manager you want to have</param>
    /// <param name="projectId">Project for which you want to have new manager</param>
    /// <returns>true on success</returns>
    public bool UpdateProjectManager(long userId, long projectId) {
        var result = RolesRepository.UpdateProjectManager(projectId, userId);
        return result;
    }

    /// <summary>
    /// Updates the roles of specified user. New roles can be assigned, or otherwise based on the input role list
    /// </summary>
    /// <param name="roleAssignments">list of all roles the user should have</param>
    /// <param name="userId">id of the user for which you want to update roles</param>
    public void UpdateUserRoleAssignments(List<DataModelAssignmentPL<RolePL>> roleAssignments, long userId) {
        var userRoles = RolesRepository.GetUserRolesByUserId(userId).ToArray();

        List<long> unassignList = new();
        List<long> assignList = new();
        foreach (var roleAssignment in roleAssignments) {
            if (roleAssignment.IsAssigned) {
                if (userRoles.All(role => role.Id != roleAssignment.DataModel.Id)) {
                    //we need to add reference
                    assignList.Add(roleAssignment.DataModel.Id);
                }
                //nothing changed
            }
            else {
                if (userRoles.Any(role => role.Id == roleAssignment.DataModel.Id)) {
                    //we need to remove reference
                    unassignList.Add(roleAssignment.DataModel.Id);
                }
            }
        }

        if (assignList.Count > 0) {
            RolesRepository.AssignRolesToUser(assignList, userId);
        }

        if (unassignList.Count > 0) {
            RolesRepository.UnassignRolesFromUser(unassignList, userId);
        }
    }


    /// <summary>
    /// Returns all roles passed user have
    /// </summary>
    /// <param name="userId">user you have to collect role for</param>
    /// <returns>list of all assigned roles</returns>
    public IEnumerable<DataModelAssignmentPL<RolePL>>? GetAllRolesAssigned(long userId) {
        var allRoles = RolesRepository.GetAllNonProjectManRoles();
        var userRoles = RolesRepository.GetUserRolesByUserId(userId);
        if (!allRoles.Any()) {
            return Array.Empty<DataModelAssignmentPL<RolePL>>();
        }

        var result = from role in allRoles
            let assigned = userRoles.Any(x => x.Id == role.Id)
            select new DataModelAssignmentPL<RolePL>(assigned, Mapper.Map<RolePL>(role));

        return result;
    }


    /// <summary>
    /// Returns role object for specified role type
    /// </summary>
    /// <param name="roleType">Role type you want to collect</param>
    /// <returns>Role object from data source</returns>
    public RolePL? GetRoleByType(RoleType roleType) {
        if (roleType == RoleType.NoRole) {
            return null;
        }

        var role = RolesRepository.GetRolesByType(roleType).FirstOrDefault();
        return Mapper.Map<RolePL>(role);
    }


    /// <summary>
    /// Method collect the user from data source by project manager role 
    /// </summary>
    /// <param name="projectId">id of the project for which a project manager should be obtained</param>
    /// <returns>Project manager, null if not existing</returns>
    public UserBasePL? GetManagerByProjectId(long projectId) {

        var role = RolesRepository.GetRoleByProjectId(projectId);
        if (role == null) {
            return null;
        }

        var user = UserRepository.GetProjectManagerByRoleId(role.Id);
        
        return user == null ? null : Mapper.Map<UserBasePL>(user);
    }
}