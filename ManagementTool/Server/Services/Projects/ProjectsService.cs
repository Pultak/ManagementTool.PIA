using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Server.Services.Roles;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;
using ManagementTool.Shared.Models.Presentation.Api.Requests;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Server.Services.Projects;

public class ProjectsService : IProjectsService {
    public ProjectsService(IProjectRepository projectRepository, IRolesService rolesService,
        IAssignmentRepository assignmentRepository, IUserRoleRepository rolesRepository,
        IUserRepository userRepository, IMapper mapper) {
        ProjectRepository = projectRepository;
        RolesService = rolesService;
        AssignmentRepository = assignmentRepository;
        RolesRepository = rolesRepository;
        UserRepository = userRepository;
        Mapper = mapper;
    }

    private IProjectRepository ProjectRepository { get; }
    private IRolesService RolesService { get; }
    private IAssignmentRepository AssignmentRepository { get; }
    private IUserRoleRepository RolesRepository { get; }
    private IUserRepository UserRepository { get; }
    private IMapper Mapper { get; }



    /// <summary>
    /// Creates project from passed data. Firstly it is validated and return error creation response if invalid
    /// </summary>
    /// <param name="project">project object you want to create in data source</param>
    /// <returns>Project creation response enum, ok if successful</returns>
    public ProjectCreationResponse CreateProject(ProjectUpdateRequest project) {
        var blProject = Mapper.Map<ProjectBLL>(project.Project);

        if (string.IsNullOrEmpty(blProject.ProjectName)) {
            return ProjectCreationResponse.InvalidName;
        }

        var valResult = ProjectUtils.ValidateNewProject(project.Project);
        if (valResult != ProjectCreationResponse.Ok) {
            return valResult;
        }

        valResult = CheckProjectDataConflicts(blProject);
        if (valResult != ProjectCreationResponse.Ok) {
            return valResult;
        }

        var projectId = ProjectRepository.AddProject(blProject);
        if (projectId < 1) {
            return ProjectCreationResponse.EmptyProject;
        }
        var roleOk = RolesService.CreateNewProjectRole(blProject.ProjectName, projectId);

        var managerAssigned = RolesService.UpdateProjectManager(project.ProjectManagerId, projectId);

        return roleOk && managerAssigned ? ProjectCreationResponse.Ok : ProjectCreationResponse.InvalidRoleName;
    }


    /// <summary>
    /// Method for updating of existing project. It checks the validity of the new data and updates it in the datasource
    /// </summary>
    /// <param name="project"></param>
    /// <returns>true on success</returns>
    public bool UpdateProject(ProjectUpdateRequest project) {
        var blProject = Mapper.Map<ProjectBLL>(project.Project);

        if (string.IsNullOrEmpty(blProject.ProjectName)) {
            return false;
        }

        var valResult = ProjectUtils.ValidateNewProject(project.Project);
        if (valResult != ProjectCreationResponse.Ok) {
            return false;
        }

        var updateOk = ProjectRepository.UpdateProject(blProject);

        if (!updateOk) {
            return false;
        }

        updateOk = RolesService.UpdateProjectRoleName(blProject.ProjectName, blProject.Id);
        var managerOk = RolesService.UpdateProjectManager(project.ProjectManagerId, project.Project.Id);

        return updateOk && managerOk;
    }

    /// <summary>
    /// Method for deletion of project. The project must be present in data source
    /// </summary>
    /// <param name="projectId">Id of project you want to delete</param>
    /// <returns>true on success, false otherwise</returns>
    public bool DeleteProject(long projectId) {
        if (projectId < 1) {
            return false;
        }

        var project = ProjectRepository.GetProjectById(projectId);
        if (project == null) {
            //no such project found
            return false;
        }

        var deletionOk = ProjectRepository.DeleteProject(project.Id) &&
                         ProjectRepository.DeleteProjectUserAssignments(project.Id) &&
                         //change all assignment ids to 0 so that it indicates there is no valid assigned project anymore
                         AssignmentRepository.UpdateAssignmentProjectIds(projectId, 0) &&
                         RolesRepository.DeleteProjectRole(project.Id);
        return deletionOk;
    }

    /// <summary>
    /// Updates the assignments to project for specified users.
    /// New users can be assigned, or otherwise based on the input project list
    /// </summary>
    /// <param name="users">list of users that should be assigned to project</param>
    /// <param name="projectId">id of the project you a project assignment for</param>
    /// <returns>true if successful</returns>
    public bool AssignUsersToProject(IEnumerable<UserBasePL> users, long projectId) {
        var blUsers = Mapper.Map<IEnumerable<UserBaseBLL>>(users);

        if (projectId < 0) {
            return false;
        }

        var project = ProjectRepository.GetProjectById(projectId);
        if (project == null) {
            return false;
        }

        var ok = UpdateUserProjectAssignments(blUsers, project);
        return ok;
    }


    /// <summary>
    /// Method for retrieving all projects data source. If specified this method returns only project with specified id
    /// </summary>
    /// <param name="projectIds">ids of project you want to retrieve, null to retrieve all</param>
    /// <returns>all project objects</returns>
    public IEnumerable<ProjectPL>? GetProjects(IEnumerable<long>? projectIds) {
        IEnumerable<ProjectBLL>? projects;

        if (projectIds == null) {
            projects = ProjectRepository.GetAllProjects();
        }
        else {
            if (projectIds.Any(x => x < 1)) {
                //ids cant be negative or zero
                projects = null;
            }
            else {
                projects = ProjectRepository.GetProjectsByIds(projectIds);
            }
        }

        return Mapper.Map<IEnumerable<ProjectPL>>(projects);
    }


    /// <summary>
    /// Method for retrieving all projects data source and creates wrapper with project manager names.
    /// If specified this method returns only project with specified id
    /// </summary>
    /// <param name="projectIds">ids of project you want to retrieve, null to retrieve all</param>
    /// <returns>all project objects with manager names wrapper</returns>
    public IEnumerable<ProjectInfoPayload>? GetProjectsWithManagersInfo(IEnumerable<long>? projectIds) {
        var projects = GetProjects(projectIds);
        if (projects == null) {
            return null;
        }

        var result = new List<ProjectInfoPayload>();
        foreach (var project in projects) {
            var user = RolesService.GetManagerByProjectId(project.Id);
            result.Add(new ProjectInfoPayload{
                Project = project,
                ManagerName = user!.FullName,
                ProjectManagerId = user.Id
            });
        }

        return result;
    }


    private bool UpdateUserProjectAssignments(IEnumerable<UserBaseBLL> assignedUsers, ProjectBLL project) {
        var dbProjectAssignees = UserRepository.GetAllUsersAssignedToProjectWrappers(project.Id).ToArray();

        List<long> unassignList = new();
        List<long> assignList = new();
        foreach (var projectAssignee in dbProjectAssignees) {
            if (projectAssignee.IsAssigned) {
                if (assignedUsers.All(role => role.Id != projectAssignee.DataModel.Id)) {
                    //we need to remove reference
                    unassignList.Add(projectAssignee.DataModel.Id);
                }
                //assigned and should be assigned => nothing changed
            }
            else {
                if (assignedUsers.Any(role => role.Id == projectAssignee.DataModel.Id)) {
                    //we need to add reference
                    assignList.Add(projectAssignee.DataModel.Id);
                }
            }
        }

        if (assignList.Count > 0) {
            UserRepository.AssignUsersToProject(assignList, project.Id);
        }

        if (unassignList.Count > 0) {
            UserRepository.UnassignUsersFromProject(unassignList, project.Id);
        }

        return true;
    }


    private ProjectCreationResponse CheckProjectDataConflicts(ProjectBLL project) {
        var namedProject = ProjectRepository.GetProjectByName(project.ProjectName);
        if (namedProject != null) {
            return ProjectCreationResponse.NameTaken;
        }

        return ProjectCreationResponse.Ok;
    }
}