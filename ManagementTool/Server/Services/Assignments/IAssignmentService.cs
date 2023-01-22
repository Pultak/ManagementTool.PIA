using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;

namespace ManagementTool.Server.Services.Assignments;

public interface IAssignmentService {
    public IEnumerable<AssignmentWrapperPayload> GetAssignmentsByProjectIds(IEnumerable<long> projectIds);
    
    public bool AddNewAssignment(AssignmentPL assignment);
    public IEnumerable<AssignmentWrapperPayload> GetAssignmentsByUserId(long userId);
    public IEnumerable<AssignmentWrapperPayload> GetAllAssignments();
    public bool DeleteAssignment(long assignmentId);
    public bool UpdateAssignment(AssignmentPL assignment);
    public IEnumerable<AssignmentWrapperPayload>? GetAssignmentsUnderSuperior(long superiorId);

    public bool UpdateProjectAssignmentsId(long projectId, long newId);
}