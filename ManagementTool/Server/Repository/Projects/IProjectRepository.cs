using ManagementTool.Server.Models.Business;

namespace ManagementTool.Server.Repository.Projects;

public interface IProjectRepository {

    public IEnumerable<ProjectBLL> GetAllProjects();
    
    /// <summary>
    /// Method retrieves one project by his name
    /// </summary>
    /// <param name="name">name of the project</param>
    /// <returns>null if not present</returns>
    public ProjectBLL? GetProjectByName(string name);


    /// <summary>
    /// Method retrieves one project by his id
    /// </summary>
    /// <param name="projectId">id of the project</param>
    /// <returns>null if not present</returns>
    public ProjectBLL? GetProjectById(long projectId);


    public IEnumerable<ProjectBLL> GetProjectsByIds(IEnumerable<long> projectId);

    public long AddProject(ProjectBLL project);
    public bool DeleteProject(long id);
    public bool UpdateProject(ProjectBLL project);

    /// <summary>
    /// Checks if all users are under any of passed projects
    /// </summary>
    /// <param name="usersIds"></param>
    /// <param name="projectIds"></param>
    /// <returns></returns>
    public bool AreUsersUnderProjects(long[] usersIds, long[] projectIds);

    public bool DeleteProjectUserAssignments(long project);
    //public bool DeleteAllProjectAssignments(long project);
}