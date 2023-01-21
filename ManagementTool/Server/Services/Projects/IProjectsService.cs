using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Projects; 

public interface IProjectsService {

    public IEnumerable<Project>? GetProjects(IEnumerable<long>? projectIds);

    public EProjectCreationResponse CreateProject(Project project);

    public bool UpdateProject(Project project);
    public bool DeleteProject(long projectId);


    public bool AssignUsersToProject(IEnumerable<UserBase> users, long projectId);
}