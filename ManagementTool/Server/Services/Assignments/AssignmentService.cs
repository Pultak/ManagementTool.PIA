using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Server.Services.Assignments;

public class AssignmentService: IAssignmentService {

    private IAssignmentRepository AssignmentsRepository { get; }
    private IProjectRepository ProjectRepository{ get; }
    private IUserRepository UserRepository{ get; }
    private IMapper Mapper { get; }

    public AssignmentService(IAssignmentRepository assignRepository, IProjectRepository projectRepository,
        IUserRepository userRepository, IMapper mapper) {
        AssignmentsRepository = assignRepository;
        ProjectRepository = projectRepository;
        UserRepository = userRepository;
        Mapper = mapper;
    }

    public IEnumerable<AssignmentWrapperPayload> GetAssignmentsByProjectIds(IEnumerable<long> projectIds) {

        if (!projectIds.Any()) {
            //is empty?
            return Array.Empty<AssignmentWrapperPayload>();
        }

        var result = AssignmentsRepository.GetAssignmentsByProjectIds(projectIds);
        return Mapper.Map<IEnumerable<AssignmentWrapperPayload>>(result);
    }




    public bool AddNewAssignment(AssignmentPL assignment) {
        var blAssignment = Mapper.Map<AssignmentBLL>(assignment);

        if (ValidateAssignmentData(blAssignment) != EAssignmentCreationResponse.Ok) {
            return false;
        }

        var assignmentId = AssignmentsRepository.AddAssignment(blAssignment);
        return assignmentId < 1;
    }

    public IEnumerable<AssignmentWrapperPayload> GetAssignmentsByUserId(long userId) {
        if (userId < 1) {
            return Array.Empty<AssignmentWrapperPayload>();
        }
        var result = AssignmentsRepository.GetAssignmentsByUserId(userId);
        return Mapper.Map<IEnumerable<AssignmentWrapperPayload>>(result);
    }

    public IEnumerable<AssignmentWrapperPayload> GetAllAssignments() {
        var result = AssignmentsRepository.GetAllAssignments();
        return Mapper.Map<IEnumerable<AssignmentWrapperPayload>>(result);
    }

    public bool DeleteAssignment(long assignmentId) {
        
        if (assignmentId < 0) {
            return false;
        }

        var done = AssignmentsRepository.DeleteAssignment(assignmentId);
        return done;

    }

    public bool UpdateAssignment(AssignmentPL assignment) {
        var blAssignment = Mapper.Map<AssignmentBLL>(assignment);
        if (ValidateAssignmentData(blAssignment) != EAssignmentCreationResponse.Ok) {
            return false;
        }

        var ok = AssignmentsRepository.UpdateAssignment(blAssignment);
        return ok;
    }

    public IEnumerable<AssignmentWrapperPayload>? GetAssignmentsUnderSuperior(long superiorId) {
        if (superiorId < 0) {
            return null;
        }

        var result = AssignmentsRepository.GetAssignmentsUnderSuperior(superiorId);
        return Mapper.Map<IEnumerable<AssignmentWrapperPayload>>(result);
    }

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


    private EAssignmentCreationResponse ValidateAssignmentData(AssignmentBLL assignment) {
        
        var project = ProjectRepository.GetProjectById(assignment.ProjectId);
        if (project == null) {
            return EAssignmentCreationResponse.InvalidProject;
        }

        var user = UserRepository.GetUserById(assignment.UserId);
        if (user == null) {
            return EAssignmentCreationResponse.InvalidUser;
        }

        if (!UserRepository.IsUserAssignedToProject(user.Id, project.Id)) {
            return EAssignmentCreationResponse.UserNotAssignedToProject;
        }

        var valResult = AssignmentUtils.ValidateNewAssignment(
            Mapper.Map<AssignmentPL>(assignment), 
            Mapper.Map<ProjectPL>(project), 
            Mapper.Map<UserBasePL>(user));
        if (valResult != EAssignmentCreationResponse.Ok){
            return valResult;
        }
        return EAssignmentCreationResponse.Ok;
    }

}