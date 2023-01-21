using ManagementTool.Server.Services;
using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Repository.Projects;

public class ProjectRepository : IProjectRepository {
    private readonly ManToolDbContext _db; 

    public ProjectRepository(ManToolDbContext db) {
        _db = db;
    }

    public IEnumerable<Project> GetAllProjects() {
        return _db.Project.ToList();
    }

    public Project? GetProjectByName(string name) {
        return _db.Project.SingleOrDefault(project => string.Equals(project.ProjectName, name));
    }

    public Project? GetProjectById(long projectId) {
        return _db.Project.Find(projectId);
    }

    public IEnumerable<Project> GetProjectsByIds(IEnumerable<long> projectIds) {
        return _db.Project.Where(x => projectIds.Contains(x.Id)).ToList();
    }


    public long AddProject(Project project) {
        _db.Project.Add(project);
        var rowsChanged = _db.SaveChanges();
        if (rowsChanged <= 0) {
            return -1;
        }

        return project.Id;
    }

    public bool DeleteProject(long id) {
        var dbProject = GetProjectById(id);
        if (dbProject == null) {
            return false;
        }
        _db.Project.Remove(dbProject);
        var rowsChanged = _db.SaveChanges();

        return rowsChanged > 0;
    }
    public bool DeleteProject(Project project) {
        _db.Project.Remove(project);
        var rowsChanged = _db.SaveChanges();

        return rowsChanged > 0;
    }

    public bool UpdateProject(Project project) {
        _db.Project.Update(project);
        var rowsChanged = _db.SaveChanges();

        return rowsChanged > 0;
    }


    public bool AreUsersUnderProjects(long[] usersIds, long[] projectIds) {
        var count = _db.UserProjectXRefs.Where(x => usersIds.Contains(x.IdUser) && projectIds.Contains(x.IdProject))
            .DistinctBy(x => x.IdUser).Count();
        return count == usersIds.Length;
    }


    public bool DeleteProjectUserAssignments(long projectId) {

        var userAssignments = _db.UserProjectXRefs.Where(o => o.IdProject == projectId);
        if (!userAssignments.Any()) {
            //project can be without assignments
            return true;
        }
        _db.Remove(userAssignments);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }

    public bool DeleteAllProjectAssignments(long projectId) {

        var assignments = _db.Assignment.Where(o => o.ProjectId == projectId);
        if (!assignments.Any()) {
            //project can be without assignments
            return true;
        }
        _db.Remove(assignments);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }
}