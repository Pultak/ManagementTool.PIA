using ManagementTool.Server.Services;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Shared.Models.Api.Payloads;
using ManagementTool.Shared.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ManagementTool.Server.Repository.Projects;

public class AssignmentRepository : IAssignmentRepository {
    private readonly ManToolDbContext _db; 
    public AssignmentRepository(ManToolDbContext db) {
        _db = db;
    }


    public Assignment? GetAssignmentByName(string name) {
        return _db.Assignment.SingleOrDefault(assignment => string.Equals(assignment.Name, name));
    }

    public Assignment? GetAssignmentById(long id) {
        return _db.Assignment.Find(id);
    }


    public IEnumerable<Assignment> GetAllPlainAssignments() {
        return _db.Assignment;
    }

    public IEnumerable<AssignmentWrapper> GetAllAssignments() {
        return GetAssignmentWrappers(_db.Assignment);
    }

    public IEnumerable<AssignmentWrapper> GetAssignmentsByUserId(long userId) {
        var assignments = _db.Assignment.Where(x => x.UserId == userId);
        return GetAssignmentWrappers(assignments);
    }

    public IEnumerable<AssignmentWrapper> GetAssignmentsByProjectIds(IEnumerable<long> projectIds) {
        var assignments = _db.Assignment.Where(assign => projectIds.Contains(assign.ProjectId));
        return GetAssignmentWrappers(assignments);
    }

    public IEnumerable<Assignment> GetAssignmentsByProjectId(long projectId) {
        var assignments = _db.Assignment.Where(x => x.ProjectId == projectId);
        return assignments;
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
        
        return GetAssignmentWrappers(allSuperiorAssignments).ToList();
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
        return result.ToList();
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

    public bool UpdateAssignments(IEnumerable<Assignment> assignments) { 
        _db.Entry(assignments).State = EntityState.Modified;
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }

    public bool UpdateAssignmentProjectIds(long projectId, long newProjectId) {
        var assignments = GetAssignmentsByProjectId(projectId);
        foreach (var assignment in assignments) {
            assignment.ProjectId = newProjectId;
        }
        _db.Entry(assignments).State = EntityState.Modified;
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }



    public IEnumerable<Assignment> GetAllUsersAssignments(long[] userIds) {
        var allAssignments = _db.Assignment.Where(x => userIds.Contains(x.UserId));
        return allAssignments;
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