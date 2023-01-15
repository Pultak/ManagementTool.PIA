using ManagementTool.Shared.Models.ApiModels;
using ManagementTool.Shared.Models.Database;
using Microsoft.EntityFrameworkCore;

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

    public IEnumerable<AssignmentWrapper> GetAllAssignments() {
        return GetAssignmentWrappers(_db.Assignment);
    }

    public IEnumerable<AssignmentWrapper> GetAssignmentsByUserId(long userId) {
        var assignments = _db.Assignment.Where(x => x.UserId == userId);
        return GetAssignmentWrappers(assignments);
    }

    public IEnumerable<AssignmentWrapper> GetAssignmentsByProjectIds(List<long> projectIds) {
        var assignments = _db.Assignment.Where(assign => projectIds.Contains(assign.ProjectId));
        return GetAssignmentWrappers(assignments);
    }

    public IEnumerable<AssignmentWrapper> GetAssignmentsUnderSuperior(long superiorId) {
        var allSuperiorAssignments = _db.UserSuperiorXRefs.Where(x => x.IdSuperior == superiorId).
            Join(_db.Assignment,
            refs => refs.IdUser,
            assign => assign.UserId,
            (refs, assign) => new Assignment{
                Id = assign.Id,
                ProjectId = assign.ProjectId,
                Name = assign.Name,
                Note = assign.Note,
                UserId = assign.UserId,
                AllocationScope = assign.AllocationScope,
                FromDate = assign.FromDate,
                ToDate = assign.ToDate,
                State = assign.State
            }).Distinct();
        
        return GetAssignmentWrappers(allSuperiorAssignments);
    }

    private IEnumerable<AssignmentWrapper> GetAssignmentWrappers(IQueryable<Assignment> assignments) {
        var result = from assign in assignments
            join usr in _db.User on assign.UserId equals usr.Id
            join prj in _db.Project on assign.ProjectId equals prj.Id
            select new AssignmentWrapper
            {
                Assignment = assign,
                UserName = usr.FullName,
                ProjectName = prj.ProjectName
            };
        return result;
    }


    public long AddAssignment(Assignment assignment) {
        _db.Assignment.Add(assignment);
        var savedCount = _db.SaveChanges();
        if (savedCount <= 0) {
            return -1;
        }
        return assignment.Id;
    }

    public bool UpdateAssignment(Assignment assignment) {
        _db.Entry(assignment).State = EntityState.Modified;
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
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