using ManagementTool.Shared.Models.Api.Payloads;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Assignments; 

public interface IAssignmentService {
    IEnumerable<AssignmentWrapper> GetAssignmentsByProjectIds(IEnumerable<long> projectIds);

    protected EAssignmentCreationResponse ValidateAssignmentData(Assignment assignment);

    public bool AddNewAssignment(Assignment assignment);
    IEnumerable<AssignmentWrapper> GetAssignmentsByUserId(long userId);
    IEnumerable<AssignmentWrapper> GetAllAssignments();
    bool DeleteAssignment(long assignmentId);
    bool UpdateAssignment(Assignment assignment);
    IEnumerable<AssignmentWrapper> GetAssignmentsUnderSuperior(long superiorId);

    bool UpdateProjectAssignmentsId(long projectId, long newId);
}