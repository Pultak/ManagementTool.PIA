using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Projects;

public interface IAssignmentDataService {


    public IEnumerable<Assignment> GetAllAssignments();
    public IEnumerable<Assignment> GetAssignmentsByUserId(long userId);
    public int AddAssignment(Assignment assignment);

    public bool DeleteAssignment(Assignment assignment);
    public bool DeleteAssignment(long id);
}