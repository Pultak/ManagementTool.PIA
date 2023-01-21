using System.Collections;
using ManagementTool.Client.Pages.Projects;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Api.Payloads;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Roles; 

public class RolesService: IRolesService {
    private IProjectRepository ProjectRepository { get; }
    private IUserRoleRepository RolesRepository { get; }

    public RolesService(IProjectRepository projectRepository, IUserRoleRepository rolesRepository) {
        ProjectRepository = projectRepository;
        RolesRepository = rolesRepository;
    }

    public bool CreateNewProjectRole(string projectName, long projectId) {
        
        var newRole = new Role {
            Name = $"{projectName} Manažer",
            Type = ERoleType.ProjectManager,
            ProjectId = projectId
        };
        var roleId = RolesRepository.AddRole(newRole);
        return roleId > 0;
    }

    public bool UpdateProjectRoleName(string newProjectName, long projectId) {
        if (projectId < 1 || string.IsNullOrEmpty(newProjectName)) {
            return false;
        }

        var result = RolesRepository.UpdateProjectRoleName(projectId, projectId + " Manažer");
        return result;
    }
    
    public void UpdateUserRoleAssignments(List<DataModelAssignment<Role>> roleAssignments, long userId) {
        var userRoles = RolesRepository.GetUserRolesByUserId(userId).ToArray();
        
        List<Role> unassignList = new();
        List<Role> assignList = new();
        foreach (var roleAssignment in roleAssignments) {
            if (roleAssignment.IsAssigned) {
                if (userRoles.All(role => role.Id != roleAssignment.DataModel.Id)) {
                    //we need to add reference
                    assignList.Add(roleAssignment.DataModel);
                }
                //nothing changed
            }
            else {
                if (userRoles.Any(role => role.Id == roleAssignment.DataModel.Id)) {
                    //we need to remove reference
                    unassignList.Add(roleAssignment.DataModel);
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


    public IEnumerable<DataModelAssignment<Role>>? GetAllRolesAssigned(long userId) {
        if (userId < 1) {
            return null;
        }
        
        var allRoles = RolesRepository.GetAllNonProjectManRoles();
        var userRoles = RolesRepository.GetUserRolesByUserId(userId);
        if (!userRoles.Any()) {
            return Array.Empty<DataModelAssignment<Role>>();
        }

        var result = (from role in allRoles
            let assigned = userRoles.Any(x => x.Id == role.Id)
            select new DataModelAssignment<Role>(assigned, role));
        
        return result;
    }


    public Role? GetRoleByType(ERoleType roleType) {
        if (roleType == ERoleType.NoRole) {
            return null;
        }

        var role = RolesRepository.GetRolesByType(roleType).FirstOrDefault();
        return role;
    }

}