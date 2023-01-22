using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Server.Services.Roles;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Server.Services.Projects; 

public class ProjectsService: IProjectsService {

    private IProjectRepository ProjectRepository { get; }
    private IRolesService RolesService { get; }
    private IAssignmentRepository AssignmentRepository { get; }
    private IUserRoleRepository RolesRepository { get; }
    private IUserRepository UserRepository { get; }
    private IMapper Mapper { get; }

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


    public EProjectCreationResponse CreateProject(ProjectPL project) {

        var blProject = Mapper.Map<ProjectBLL>(project);

        if (string.IsNullOrEmpty(blProject.ProjectName)) {
            return EProjectCreationResponse.InvalidName;
        }

        var valResult = ProjectUtils.ValidateNewProject(project);
        if (valResult != EProjectCreationResponse.Ok) {
            return valResult;
        }

        valResult = CheckProjectDataConflicts(blProject);
        if (valResult != EProjectCreationResponse.Ok) {
            return valResult;
        }

        var roleOk = RolesService.CreateNewProjectRole(blProject.ProjectName, blProject.Id);
        
        
        return roleOk? EProjectCreationResponse.Ok : EProjectCreationResponse.InvalidRoleName;
    }

    
    public bool UpdateProject(ProjectPL project) {
        var blProject = Mapper.Map<ProjectBLL>(project);

        if (string.IsNullOrEmpty(blProject.ProjectName)){
            return false;
        }

        var valResult = ProjectUtils.ValidateNewProject(project);
        if (valResult != EProjectCreationResponse.Ok) {
            return false;
        }
        
        var updateOk = ProjectRepository.UpdateProject(blProject);
        
        if (!updateOk) {
            return false;
        }

        updateOk = RolesService.UpdateProjectRoleName(blProject.ProjectName, blProject.Id);

        return updateOk;
    }

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


    
    private bool UpdateUserProjectAssignments(IEnumerable<UserBaseBLL> assignedUsers, ProjectBLL project) {
        var dbProjectAssignees = UserRepository.GetAllUsersAssignedToProject(project.Id).ToArray();
        
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


    private EProjectCreationResponse CheckProjectDataConflicts(ProjectBLL project) {
        var namedProject = ProjectRepository.GetProjectByName(project.ProjectName);
        if (namedProject != null) {
            return EProjectCreationResponse.NameTaken;
        }
        return EProjectCreationResponse.Ok;
    }

}