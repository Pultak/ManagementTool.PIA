using ManagementTool.Server.Models.Business;

namespace ManagementTool.Server.Repository.Projects;

public interface IProjectRepository {

    public IEnumerable<ProjectBLL> GetAllProjects();
    public ProjectBLL? GetProjectByName(string name);
    public ProjectBLL? GetProjectById(long projectId);
    public IEnumerable<ProjectBLL> GetProjectsByIds(IEnumerable<long> projectId);

    public long AddProject(ProjectBLL project);
    public bool DeleteProject(long id);
    public bool UpdateProject(ProjectBLL project);

    public bool AreUsersUnderProjects(long[] usersIds, long[] projectIds);

    public bool DeleteProjectUserAssignments(long project);
    public bool DeleteAllProjectAssignments(long project);
}