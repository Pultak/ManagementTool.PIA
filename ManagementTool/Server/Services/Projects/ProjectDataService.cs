using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Projects;

public class ProjectDataService : IProjectDataService {
    private readonly ManToolDbContext _db; //To Get all employees details
    public ProjectDataService(ManToolDbContext db) {
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

    
    public bool DeleteProjectUserAssignments(Project project) {

        var userAssignments = _db.UserProjectXRefs.Where(o => o.IdProject == project.Id);
        if (!userAssignments.Any()) {
            //project can be without assignments
            return true;
        }
        //todo is ok?
        _db.Remove(userAssignments);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }

    public bool DeleteAllProjectAssignments(Project project) {

        var assignments = _db.Assignment.Where(o => o.ProjectId == project.Id);
        if (!assignments.Any()) {
            //project can be without assignments
            return true;
        }
        //todo is ok?
        _db.Remove(assignments);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }
}