using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Repository.Projects;

public interface IProjectRepository {

    public IEnumerable<Project> GetAllProjects();
    public Project? GetProjectByName(string name);
    public Project? GetProjectById(long projectId);
    public IEnumerable<Project> GetProjectsByIds(IEnumerable<long> projectId);

    public long AddProject(Project project);
    public bool DeleteProject(long id);
    public bool UpdateProject(Project project);

    public bool AreUsersUnderProjects(long[] usersIds, long[] projectIds);

    public bool DeleteProjectUserAssignments(long project);
    public bool DeleteAllProjectAssignments(long project);
}