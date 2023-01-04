using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Projects;

public class AssignmentDataService : IAssignmentDataService {
    private readonly ManToolDbContext _db; //To Get all employees details


    //TODO implement these bois

    public AssignmentDataService(ManToolDbContext db) {
        _db = db;
    }


    public IEnumerable<Assignment> GetAllAssignments() {
        return _db.Assignment.ToList();
    }

    public IEnumerable<Assignment> GetAssignmentsByUserId(long userId) {
        throw new NotImplementedException();
    }

    public int AddAssignment(Assignment assignment) {
        throw new NotImplementedException();
    }

    public bool DeleteAssignment(Assignment assignment) {
        throw new NotImplementedException();
    }

    public bool DeleteAssignment(long id) {
        throw new NotImplementedException();
    }
}