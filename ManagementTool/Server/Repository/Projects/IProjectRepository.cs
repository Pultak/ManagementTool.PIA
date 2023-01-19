using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Repository.Projects;

public interface IProjectRepository {

    public IEnumerable<Project> GetAllProjects();
    public Project? GetProjectByName(string name);
    public Project? GetProjectById(long projectId);
    public IEnumerable<Project> GetProjectsByIds(List<long> projectId);

    public long AddProject(Project project);
    public bool DeleteProject(long id);
    public bool DeleteProject(Project project);
    public bool UpdateProject(Project project);

    public bool AreUsersUnderProjects(long[] usersIds, long[] projectIds);

    public bool DeleteProjectUserAssignments(Project project);
    public bool DeleteAllProjectAssignments(Project project);
}