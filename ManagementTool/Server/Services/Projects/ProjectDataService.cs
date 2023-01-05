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
}