using System.Collections;
using System.Net;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Services.Roles;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;
using Microsoft.AspNetCore.Identity;

namespace ManagementTool.Server.Services.Projects; 

public class ProjectsService: IProjectsService {

    private IProjectRepository ProjectRepository { get; }
    private IRolesService RolesService { get; }
    private IAssignmentRepository AssignmentRepository { get; }
    private IUserRoleRepository RolesRepository { get; }
    private IUserRepository UserRepository { get; }

    public ProjectsService(IProjectRepository projectRepository, IRolesService rolesService, 
        IAssignmentRepository assignmentRepository, IUserRoleRepository rolesRepository, IUserRepository userRepository) {
        ProjectRepository = projectRepository;
        RolesService = rolesService;
        AssignmentRepository = assignmentRepository;
        RolesRepository = rolesRepository;
        UserRepository = userRepository;
    }


    public EProjectCreationResponse CreateProject(Project project) {

        if (string.IsNullOrEmpty(project.ProjectName)) {
            return EProjectCreationResponse.InvalidName;
        }

        var valResult = ProjectUtils.ValidateNewProject(project);
        if (valResult != EProjectCreationResponse.Ok) {
            return valResult;
        }

        valResult = CheckProjectDataConflicts(project);
        if (valResult != EProjectCreationResponse.Ok) {
            return valResult;
        }

        var roleOk = RolesService.CreateNewProjectRole(project.ProjectName, project.Id);
        
        
        return roleOk? EProjectCreationResponse.Ok : EProjectCreationResponse.InvalidRoleName;
    }

    
    public bool UpdateProject(Project project) {
        if (string.IsNullOrEmpty(project.ProjectName)){
            return false;
        }

        var valResult = ProjectUtils.ValidateNewProject(project);
        if (valResult != EProjectCreationResponse.Ok) {
            return false;
        }
        
        var updateOk = ProjectRepository.UpdateProject(project);
        
        if (!updateOk) {
            return false;
        }

        updateOk = RolesService.UpdateProjectRoleName(project.ProjectName, project.Id);

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

    public bool AssignUsersToProject(IEnumerable<UserBase> users, long projectId) {
        
        if (projectId < 0) {
            return false;
        }

        var project = ProjectRepository.GetProjectById(projectId);
        if (project == null) {
            return false;
        }
        
        var ok = UpdateUserProjectAssignments(users, project);
        return ok;
    }


    public IEnumerable<Project>? GetProjects(IEnumerable<long>? projectIds) {

        IEnumerable<Project>? projects;

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

        return projects;
    }


    
    private bool UpdateUserProjectAssignments(IEnumerable<UserBase> assignedUsers, Project project) {
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
            UserRepository.AssignUsersToProject(assignList, project);
        }

        if (unassignList.Count > 0) {
            UserRepository.UnassignUsersFromProject(unassignList, project);
        }

        return true;
    }


    private EProjectCreationResponse CheckProjectDataConflicts(Project project) {
        var namedProject = ProjectRepository.GetProjectByName(project.ProjectName);
        if (namedProject != null) {
            return EProjectCreationResponse.NameTaken;
        }
        return EProjectCreationResponse.Ok;
    }

}