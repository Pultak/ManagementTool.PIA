using System.Net;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Api.Payloads;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Server.Services.Assignments; 

public class AssignmentService: IAssignmentService {

    private IAssignmentRepository AssignmentsRepository { get; }
    private IProjectRepository ProjectRepository{ get; }
    private IUserRepository UserRepository{ get; }

    public AssignmentService(IAssignmentRepository assignRepository, IProjectRepository projectRepository, IUserRepository userRepository) {
        AssignmentsRepository = assignRepository;
        ProjectRepository = projectRepository;
        UserRepository = userRepository;
    }

    public IEnumerable<AssignmentWrapper> GetAssignmentsByProjectIds(IEnumerable<long> projectIds) {

        if (projectIds.Any()) {
            //is empty?
            return Array.Empty<AssignmentWrapper>();
        }

        return AssignmentsRepository.GetAssignmentsByProjectIds(projectIds);
    }




    public bool AddNewAssignment(Assignment assignment) {
        if (ValidateAssignmentData(assignment) != EAssignmentCreationResponse.Ok) {
            return false;
        }

        var assignmentId = AssignmentsRepository.AddAssignment(assignment);
        return assignmentId < 1;
    }

    public IEnumerable<AssignmentWrapper> GetAssignmentsByUserId(long userId) {
        if (userId < 1) {
            return Array.Empty<AssignmentWrapper>();
        }
        var result = AssignmentsRepository.GetAssignmentsByUserId((long)userId);
        return result;
    }

    public IEnumerable<AssignmentWrapper> GetAllAssignments() {
        return AssignmentsRepository.GetAllAssignments();
    }

    public bool DeleteAssignment(long assignmentId) {
        
        if (assignmentId < 0) {
            return false;
        }

        var done = AssignmentsRepository.DeleteAssignment(assignmentId);
        return done;

    }

    public bool UpdateAssignment(Assignment assignment) {
        if (ValidateAssignmentData(assignment) != EAssignmentCreationResponse.Ok) {
            return false;
        }

        var ok = AssignmentsRepository.UpdateAssignment(assignment);
        return ok;
    }

    public IEnumerable<AssignmentWrapper> GetAssignmentsUnderSuperior(long superiorId) => throw new NotImplementedException();

    public bool UpdateProjectAssignmentsId(long projectId, long newId) {
        if (projectId < 1 || newId < 0) {
            return false;
        }

        var allProjectAssignments = AssignmentsRepository.GetAssignmentsByProjectId(projectId);

        foreach (var assignment in allProjectAssignments) {
            assignment.ProjectId = newId;
        }

        var updateOk = AssignmentsRepository.UpdateAssignments(allProjectAssignments);
        return updateOk;
    }


    public EAssignmentCreationResponse ValidateAssignmentData(Assignment assignment) {
        
        var project = ProjectRepository.GetProjectById(assignment.ProjectId);
        if (project == null) {
            return EAssignmentCreationResponse.InvalidProject;
        }

        var user = UserRepository.GetUserById(assignment.UserId);
        if (user == null) {
            return EAssignmentCreationResponse.InvalidUser;
        }

        if (!UserRepository.IsUserAssignedToProject(user, project)) {
            return EAssignmentCreationResponse.UserNotAssignedToProject;
        }

        var valResult = AssignmentUtils.ValidateNewAssignment(assignment, project, user);
        if (valResult != EAssignmentCreationResponse.Ok){
            return valResult;
        }
        return EAssignmentCreationResponse.Ok;
    }

}