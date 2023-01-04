using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Projects;

public class ProjectDataService : IProjectDataService {
    private readonly ManToolDbContext _db; //To Get all employees details


    //TODO implement these bois

    public ProjectDataService(ManToolDbContext db) {
        _db = db;
    }

    public IEnumerable<Project> GetAllProjects() {
        return _db.Project.ToList();
    }

    public Project? GetProjectByName(string name) {
        throw new NotImplementedException();
    }

    public Project? GetProjectById(long projectId) {
        throw new NotImplementedException();
    }

    public int AddProject(Project project) {
        throw new NotImplementedException();
    }

    public void DeleteProject(long id) {
        throw new NotImplementedException();
    }

    public void UpdateProject(Project project) {
        throw new NotImplementedException();
    }
}