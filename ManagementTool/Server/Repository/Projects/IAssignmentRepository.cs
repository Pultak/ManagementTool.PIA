using ManagementTool.Server.Models.Business;

namespace ManagementTool.Server.Repository.Projects;

public interface IAssignmentRepository {
    
    public AssignmentBLL? GetAssignmentByName(string name);
    public AssignmentBLL? GetAssignmentById(long id);
    public IEnumerable<AssignmentBLL> GetAllPlainAssignments();
    public IEnumerable<AssignmentWrapperBLL> GetAllAssignments();
    public IEnumerable<AssignmentWrapperBLL> GetAssignmentsByUserId(long userId);
    public IEnumerable<AssignmentWrapperBLL> GetAssignmentsByProjectIds(IEnumerable<long> projectIds);
    public IEnumerable<AssignmentBLL> GetAssignmentsByProjectId(long projectId);

    public bool UpdateAssignmentProjectIds(long projectId, long newProjectId);

    public IEnumerable<AssignmentWrapperBLL> GetAssignmentsUnderSuperior(long superiorId);
    public long AddAssignment(AssignmentBLL assignment);

    public bool UpdateAssignment(AssignmentBLL assignment);
    public bool UpdateAssignments(IEnumerable<AssignmentBLL> assignments);


    public IEnumerable<AssignmentBLL> GetAllUsersAssignments(long[] userIds);

    public bool DeleteAssignment(AssignmentBLL assignment);
    public bool DeleteAssignment(long id);
}