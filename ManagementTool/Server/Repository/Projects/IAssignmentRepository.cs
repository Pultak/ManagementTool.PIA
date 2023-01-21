using ManagementTool.Shared.Models.Api.Payloads;
using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Projects;

public interface IAssignmentRepository {
    
    public Assignment? GetAssignmentByName(string name);
    public Assignment? GetAssignmentById(long id);
    public IEnumerable<Assignment> GetAllPlainAssignments();
    public IEnumerable<AssignmentWrapper> GetAllAssignments();
    public IEnumerable<AssignmentWrapper> GetAssignmentsByUserId(long userId);
    public IEnumerable<AssignmentWrapper> GetAssignmentsByProjectIds(IEnumerable<long> projectIds);
    public IEnumerable<Assignment> GetAssignmentsByProjectId(long projectId);

    public bool UpdateAssignmentProjectIds(long projectId, long newProjectId);

    public IEnumerable<AssignmentWrapper> GetAssignmentsUnderSuperior(long superiorId);
    public long AddAssignment(Assignment assignment);

    public bool UpdateAssignment(Assignment assignment);
    public bool UpdateAssignments(IEnumerable<Assignment> assignments);


    public IEnumerable<Assignment> GetAllUsersAssignments(long[] userIds);

    public bool DeleteAssignment(Assignment assignment);
    public bool DeleteAssignment(long id);
}