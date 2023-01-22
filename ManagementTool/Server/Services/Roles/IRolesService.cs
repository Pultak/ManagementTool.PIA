using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Roles; 

public interface IRolesService {
    public bool CreateNewProjectRole(string projectName, long projectId);

    public bool UpdateProjectRoleName(string projectName, long projectId);

    public void UpdateUserRoleAssignments(List<DataModelAssignmentPL<RolePL>> roleAssignments, long userId);
    public IEnumerable<DataModelAssignmentPL<RolePL>>? GetAllRolesAssigned(long userId);

    public RolePL? GetRoleByType(ERoleType roleType);
}