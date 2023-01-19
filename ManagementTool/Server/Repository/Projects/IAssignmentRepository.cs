using ManagementTool.Shared.Models.ApiModels;
using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Projects;

public interface IAssignmentRepository {
    
    public Assignment? GetAssignmentByName(string name);
    public Assignment? GetAssignmentById(long id);
    public IEnumerable<Assignment> GetAllPlainAssignments();
    public IEnumerable<AssignmentWrapper> GetAllAssignments();
    public IEnumerable<AssignmentWrapper> GetAssignmentsByUserId(long userId);
    public IEnumerable<AssignmentWrapper> GetAssignmentsByProjectIds(List<long> projectIds);
    public IEnumerable<AssignmentWrapper> GetAssignmentsUnderSuperior(long superiorId);
    public long AddAssignment(Assignment assignment);

    public bool UpdateAssignment(Assignment assignment);


    public IEnumerable<Assignment> GetAllUsersAssignments(long[] userIds);

    public bool DeleteAssignment(Assignment assignment);
    public bool DeleteAssignment(long id);
}