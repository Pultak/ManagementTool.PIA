using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Projects; 

public interface IProjectsService {

    public IEnumerable<ProjectPL>? GetProjects(IEnumerable<long>? projectIds);

    public EProjectCreationResponse CreateProject(ProjectPL project);

    public bool UpdateProject(ProjectPL project);
    public bool DeleteProject(long projectId);

    public bool AssignUsersToProject(IEnumerable<UserBasePL> users, long projectId);
}