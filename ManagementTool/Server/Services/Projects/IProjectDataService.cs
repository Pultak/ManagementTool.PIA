using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Projects;

public interface IProjectDataService {

    public IEnumerable<Project> GetAllProjects();
    public Project? GetProjectByName(string name);
    public Project? GetProjectById(long projectId);

    public int AddProject(Project project);
    public void DeleteProject(long id);
    public void UpdateProject(Project project);
}