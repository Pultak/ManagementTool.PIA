using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Server.Services.Assignments;

public class AssignmentService : IAssignmentService {
    public AssignmentService(IAssignmentRepository assignRepository, IProjectRepository projectRepository,
        IUserRepository userRepository, IMapper mapper) {
        AssignmentsRepository = assignRepository;
        ProjectRepository = projectRepository;
        UserRepository = userRepository;
        Mapper = mapper;
    }

    private IAssignmentRepository AssignmentsRepository { get; }
    private IProjectRepository ProjectRepository { get; }
    private IUserRepository UserRepository { get; }
    private IMapper Mapper { get; }
    /// <summary>
    /// Method get all assignments under specified projects 
    /// </summary>
    /// <param name="projectIds">all project you want assignments of</param>
    /// <returns>all assignments with wrapped with project and user names</returns>
    public IEnumerable<AssignmentWrapperPayload> GetAssignmentsByProjectIds(IEnumerable<long> projectIds) {
        if (!projectIds.Any()) {
            //is empty?
            return Array.Empty<AssignmentWrapperPayload>();
        }

        var result = AssignmentsRepository.GetAssignmentsByProjectIds(projectIds);
        return Mapper.Map<IEnumerable<AssignmentWrapperPayload>>(result);
    }

    /// <summary>
    /// Method for creation of new valid assignment. Assignment is firstly checked for validity and then checked for any data conflicts
    /// </summary>
    /// <param name="assignment">new assignment object you want to create</param>
    /// <returns>True on success, false otherwise</returns>
    public bool AddNewAssignment(AssignmentPL assignment) {
        var blAssignment = Mapper.Map<AssignmentBLL>(assignment);

        if (ValidateAssignmentData(blAssignment) != AssignmentCreationResponse.Ok) {
            return false;
        }

        var assignmentId = AssignmentsRepository.AddAssignment(blAssignment);
        return assignmentId < 1;
    }

    /// <summary>
    /// Method returns all assignments that are assigned to specified user
    /// </summary>
    /// <param name="userId">id of user</param>
    /// <returns>all assignments with wrapped  project and user names</returns>
    public IEnumerable<AssignmentWrapperPayload> GetAssignmentsByUserId(long userId) {
        if (userId < 1) {
            return Array.Empty<AssignmentWrapperPayload>();
        }

        var result = AssignmentsRepository.GetAssignmentsByUserId(userId);
        return Mapper.Map<IEnumerable<AssignmentWrapperPayload>>(result);
    }
    /// <summary>
    /// Method returns all assignments you can find inside of data source
    /// </summary>
    /// <returns>all assignments with wrapped  project and user names</returns>
    public IEnumerable<AssignmentWrapperPayload> GetAllAssignments() {
        var result = AssignmentsRepository.GetAllAssignments();
        return Mapper.Map<IEnumerable<AssignmentWrapperPayload>>(result);
    }
    /// <summary>
    /// Method deletes the assignment but it needs to be present in the data store
    /// </summary>
    /// <param name="assignmentId"></param>
    /// <returns>True on success, false otherwise</returns>
    public bool DeleteAssignment(long assignmentId) {
        if (assignmentId < 0) {
            return false;
        }

        var done = AssignmentsRepository.DeleteAssignment(assignmentId);
        return done;
    }
    /// <summary>
    /// Method updates the assignment if the data passed is valid and there is no conflict inside the data source
    /// </summary>
    /// <param name="assignment">assignment you want to update</param>
    /// <returns>True on success, false otherwise</returns>
    public bool UpdateAssignment(AssignmentPL assignment) {
        var blAssignment = Mapper.Map<AssignmentBLL>(assignment);
        if (ValidateAssignmentData(blAssignment) != AssignmentCreationResponse.Ok) {
            return false;
        }

        var ok = AssignmentsRepository.UpdateAssignment(blAssignment);
        return ok;
    }
    /// <summary>
    /// Method returns all assignments that are under superiors power to control
    /// </summary>
    /// <param name="superiorId"></param>
    /// <returns>all assignments with wrapped project and user names</returns>
    public IEnumerable<AssignmentWrapperPayload>? GetAssignmentsUnderSuperior(long superiorId) {
        if (superiorId < 0) {
            return null;
        }

        var result = AssignmentsRepository.GetAssignmentsUnderSuperior(superiorId);
        return Mapper.Map<IEnumerable<AssignmentWrapperPayload>>(result);
    }

    /* this method could be used if project deletion was present in the system
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
    }*/


    private AssignmentCreationResponse ValidateAssignmentData(AssignmentBLL assignment) {
        var project = ProjectRepository.GetProjectById(assignment.ProjectId);
        if (project == null) {
            return AssignmentCreationResponse.InvalidProject;
        }

        var user = UserRepository.GetUserById(assignment.UserId);
        if (user == null) {
            return AssignmentCreationResponse.InvalidUser;
        }

        if (!UserRepository.IsUserAssignedToProject(user.Id, project.Id)) {
            return AssignmentCreationResponse.UserNotAssignedToProject;
        }

        var valResult = AssignmentUtils.ValidateNewAssignment(
            Mapper.Map<AssignmentPL>(assignment),
            Mapper.Map<ProjectPL>(project),
            Mapper.Map<UserBasePL>(user));
        if (valResult != AssignmentCreationResponse.Ok) {
            return valResult;
        }

        return AssignmentCreationResponse.Ok;
    }
}