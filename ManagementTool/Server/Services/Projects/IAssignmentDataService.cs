using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Projects;

public interface IAssignmentDataService {
    
    public Assignment? GetAssignmentByName(string name);
    public Assignment? GetAssignmentById(long id);
    public IEnumerable<Assignment> GetAllAssignments();
    public IEnumerable<Assignment> GetAssignmentsByUserId(long userId);
    public long AddAssignment(Assignment assignment);

    public bool DeleteAssignment(Assignment assignment);
    public bool DeleteAssignment(long id);
}