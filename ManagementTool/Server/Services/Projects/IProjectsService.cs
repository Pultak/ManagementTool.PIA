using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;
using ManagementTool.Shared.Models.Presentation.Api.Requests;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Projects;

public interface IProjectsService {
    /// <summary>
    /// Method for retrieving all projects data source. If specified this method returns only project with specified id
    /// </summary>
    /// <param name="projectIds">ids of project you want to retrieve, null to retrieve all</param>
    /// <returns>all project objects</returns>
    public IEnumerable<ProjectPL>? GetProjects(IEnumerable<long>? projectIds);

    /// <summary>
    /// Method for retrieving all projects data source and creates wrapper with project manager names.
    /// If specified this method returns only project with specified id
    /// </summary>
    /// <param name="projectIds">ids of project you want to retrieve, null to retrieve all</param>
    /// <returns>all project objects with manager names wrapper</returns>
    public IEnumerable<ProjectInfoPayload>? GetProjectsWithManagersInfo(IEnumerable<long>? projectIds);

    /// <summary>
    /// Creates project from passed data. Firstly it is validated and return error creation response if invalid
    /// </summary>
    /// <param name="project">project object you want to create in data source</param>
    /// <returns>Project creation response enum, ok if successful</returns>
    public ProjectCreationResponse CreateProject(ProjectUpdateRequest project);


    /// <summary>
    /// Method for updating of existing project. It checks the validity of the new data and updates it in the datasource
    /// </summary>
    /// <param name="project"></param>
    /// <returns>true on success</returns>
    public bool UpdateProject(ProjectUpdateRequest project);

    /// <summary>
    /// Method for deletion of project. The project must be present in data source
    /// </summary>
    /// <param name="projectId">Id of project you want to delete</param>
    /// <returns>true on success, false otherwise</returns>
    public bool DeleteProject(long projectId);

    /// <summary>
    /// Updates the assignments to project for specified users.
    /// New users can be assigned, or otherwise based on the input project list
    /// </summary>
    /// <param name="users">list of users that should be assigned to project</param>
    /// <param name="projectId">id of the project you a project assignment for</param>
    /// <returns>true if successful</returns>
    public bool AssignUsersToProject(IEnumerable<UserBasePL> users, long projectId);
}