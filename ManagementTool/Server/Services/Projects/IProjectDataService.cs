using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Projects;

public interface IProjectDataService {

    public IEnumerable<Project> GetAllProjects();
    public Project? GetProjectByName(string name);
    public Project? GetProjectById(long projectId);

    public long AddProject(Project project);
    public bool DeleteProject(long id);
    public bool DeleteProject(Project project);
    public bool UpdateProject(Project project);
}