using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Projects;

public class AssignmentDataService : IAssignmentDataService {
    private readonly ManToolDbContext _db; 
    public AssignmentDataService(ManToolDbContext db) {
        _db = db;
    }


    public Assignment? GetAssignmentByName(string name) {
        return _db.Assignment.SingleOrDefault(assignment => string.Equals(assignment.Name, name));
    }

    public Assignment? GetAssignmentById(long id) {
        return _db.Assignment.Find(id);
    }

    public IEnumerable<Assignment> GetAllAssignments() {
        return _db.Assignment.ToList();
    }

    public IEnumerable<Assignment> GetAssignmentsByUserId(long userId) {
        return _db.Assignment.Where(x => x.UserId == userId).ToList();
    }

    public long AddAssignment(Assignment assignment) {
        _db.Assignment.Add(assignment);
        var savedCount = _db.SaveChanges();
        if (savedCount <= 0) {
            return -1;
        }
        return assignment.Id;
    }

    public bool DeleteAssignment(Assignment assignment) {
        _db.Assignment.Remove(assignment);
        var rowsChanged = _db.SaveChanges();

        return rowsChanged > 0;
    }

    public bool DeleteAssignment(long id) {
        var dbAssignment = GetAssignmentById(id);
        if (dbAssignment == null) {
            return false;
        }
        _db.Assignment.Remove(dbAssignment);
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }
}