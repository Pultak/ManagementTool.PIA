using ManagementTool.Shared.Models.Api.Payloads;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Roles; 

public interface IRolesService {
    public bool CreateNewProjectRole(string projectName, long projectId);

    public bool UpdateProjectRoleName(string projectName, long projectId);

    public void UpdateUserRoleAssignments(List<DataModelAssignment<Role>> roleAssignments, long userId);
    public IEnumerable<DataModelAssignment<Role>>? GetAllRolesAssigned(long userId);

    public Role? GetRoleByType(ERoleType roleType);
}