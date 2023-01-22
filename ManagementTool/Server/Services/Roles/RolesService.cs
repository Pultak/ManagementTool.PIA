using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Roles; 

public class RolesService: IRolesService {
    private IUserRoleRepository RolesRepository { get; }
    private IMapper Mapper { get; }

    public RolesService(IUserRoleRepository rolesRepository, IMapper mapper) {
        RolesRepository = rolesRepository;
        Mapper = mapper;
    }

    public bool CreateNewProjectRole(string projectName, long projectId) {
        
        var newRole = new RoleBLL {
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


    public IEnumerable<DataModelAssignmentPL<RolePL>>? GetAllRolesAssigned(long userId) {
        if (userId < 1) {
            return null;
        }
        
        var allRoles = RolesRepository.GetAllNonProjectManRoles();
        var userRoles = RolesRepository.GetUserRolesByUserId(userId);
        if (!userRoles.Any()) {
            return Array.Empty<DataModelAssignmentPL<RolePL>>();
        }

        var result = from role in allRoles
            let assigned = userRoles.Any(x => x.Id == role.Id)
            select new DataModelAssignmentPL<RolePL>(assigned, Mapper.Map<RolePL>(role));
        
        return result;
    }


    public RolePL? GetRoleByType(ERoleType roleType) {
        if (roleType == ERoleType.NoRole) {
            return null;
        }

        var role = RolesRepository.GetRolesByType(roleType).FirstOrDefault();
        return Mapper.Map<RolePL>(role);
    }

}