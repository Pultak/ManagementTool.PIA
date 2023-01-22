using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ManagementTool.Server.Repository.Projects;

public class AssignmentRepository : IAssignmentRepository {
    private readonly ManToolDbContext _db;
    private IMapper Mapper { get;}
    
    public AssignmentRepository(ManToolDbContext db, IMapper mapper) {
        _db = db;
        Mapper = mapper;
    }


    public AssignmentBLL? GetAssignmentByName(string name) {
        var result = _db.Assignment?.SingleOrDefault(assignment => string.Equals(assignment.Name, name));
        return result != null ? Mapper.Map<AssignmentBLL>(result) : null;
    }

    public AssignmentBLL? GetAssignmentById(long id) {
        var result = _db.Assignment?.Find(id);
        return result != null ? Mapper.Map<AssignmentBLL>(result) : null;
    }


    public IEnumerable<AssignmentBLL> GetAllPlainAssignments() {
        var result = _db.Assignment;
        if (result == null) {
            return Array.Empty<AssignmentBLL>();
        }

        return Mapper.Map<AssignmentBLL[]>(result);
    }

    public IEnumerable<AssignmentWrapperBLL> GetAllAssignments() {
        return GetAssignmentWrappers(_db.Assignment);
    }

    public IEnumerable<AssignmentWrapperBLL> GetAssignmentsByUserId(long userId) {
        var assignments = _db.Assignment?.Where(x => x.UserId == userId);
        return GetAssignmentWrappers(assignments);
    }

    public IEnumerable<AssignmentWrapperBLL> GetAssignmentsByProjectIds(IEnumerable<long> projectIds) {
        var assignments = _db.Assignment?.Where(assign => projectIds.Contains(assign.ProjectId));
        return GetAssignmentWrappers(assignments);
    }

    public IEnumerable<AssignmentBLL> GetAssignmentsByProjectId(long projectId) {
        var assignments = _db.Assignment?.Where(x => x.ProjectId == projectId);
        return assignments == null ? Enumerable.Empty<AssignmentBLL>() : Mapper.Map<AssignmentBLL[]>(assignments);
    }


    public IEnumerable<AssignmentWrapperBLL> GetAssignmentsUnderSuperior(long superiorId) {
        if (_db.Assignment == null) {
            return Enumerable.Empty<AssignmentWrapperBLL>();
        }
        var allSuperiorAssignments = _db.UserSuperiorXRefs?.Where(x => x.IdSuperior == superiorId).
            Join(_db.Assignment,
            refs => refs.IdUser,
            assign => assign.UserId,
            (refs, assign) => new AssignmentDAL{
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

    private IEnumerable<AssignmentWrapperBLL> GetAssignmentWrappers(IQueryable<AssignmentDAL>? assignments) {
        if (assignments == null) {
            return Enumerable.Empty<AssignmentWrapperBLL>();
        }
        var result = from assign in assignments
            join usr in _db.User on assign.UserId equals usr.Id
            join prj in _db.Project on assign.ProjectId equals prj.Id
            select new AssignmentWrapperBLL
            {
                Assignment = Mapper.Map<AssignmentBLL>(assign),
                UserName = usr.FullName,
                ProjectName = prj.ProjectName
            };
        return result.ToList();
    }


    public long AddAssignment(AssignmentBLL assignment) {
        _db.Assignment?.Add(Mapper.Map<AssignmentDAL>(assignment));
        var savedCount = _db.SaveChanges();
        if (savedCount <= 0) {
            return -1;
        }
        return assignment.Id;
    }

    public bool UpdateAssignment(AssignmentBLL assignment) {
        _db.Entry(Mapper.Map<AssignmentDAL>(assignment)).State = EntityState.Modified;
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }

    public bool UpdateAssignments(IEnumerable<AssignmentBLL> assignments) {
        _db.Entry(Mapper.Map<AssignmentDAL[]>(assignments)).State = EntityState.Modified;
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }

    public bool UpdateAssignmentProjectIds(long projectId, long newProjectId) {
        var assignments = GetAssignmentsByProjectId(projectId);
        foreach (var assignment in assignments) {
            assignment.ProjectId = newProjectId;
        }
        _db.Entry(Mapper.Map<AssignmentDAL[]>(assignments)).State = EntityState.Modified;
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }



    public IEnumerable<AssignmentBLL> GetAllUsersAssignments(long[] userIds) {
        var allAssignments = _db.Assignment?.Where(x => userIds.Contains(x.UserId));
        return allAssignments == null ? Enumerable.Empty<AssignmentBLL>() : Mapper.Map<AssignmentBLL[]>(allAssignments);
    }


    public bool DeleteAssignment(AssignmentBLL assignment) {
        _db.Assignment?.Remove(Mapper.Map<AssignmentDAL>(assignment));
        var rowsChanged = _db.SaveChanges();

        return rowsChanged > 0;
    }



    public bool DeleteAssignment(long id) {
        var dbAssignment = _db.Assignment?.Find(id);
        if (dbAssignment == null) {
            return false;
        }
        _db.Assignment?.Remove(dbAssignment);
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }
}