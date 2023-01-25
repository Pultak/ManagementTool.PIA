using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;

namespace ManagementTool.Server.Services.Assignments;

public interface IAssignmentService {
    /// <summary>
    /// Method get all assignments under specified projects 
    /// </summary>
    /// <param name="projectIds">all project you want assignments of</param>
    /// <returns>all assignments with wrapped with project and user names</returns>
    public IEnumerable<AssignmentWrapperPayload> GetAssignmentsByProjectIds(IEnumerable<long> projectIds);
    /// <summary>
    /// Method for creation of new valid assignment. Assignment is firstly checked for validity and then checked for any data conflicts
    /// </summary>
    /// <param name="assignment">new assignment object you want to create</param>
    /// <returns>True on success, false otherwise</returns>
    public bool AddNewAssignment(AssignmentPL assignment);
    /// <summary>
    /// Method returns all assignments that are assigned to specified user
    /// </summary>
    /// <param name="userId">id of user</param>
    /// <returns>all assignments with wrapped  project and user names</returns>
    public IEnumerable<AssignmentWrapperPayload> GetAssignmentsByUserId(long userId);
    /// <summary>
    /// Method returns all assignments you can find inside of data source
    /// </summary>
    /// <returns>all assignments with wrapped  project and user names</returns>
    public IEnumerable<AssignmentWrapperPayload> GetAllAssignments();
    /// <summary>
    /// Method deletes the assignment but it needs to be present in the data store
    /// </summary>
    /// <param name="assignmentId"></param>
    /// <returns>True on success, false otherwise</returns>
    public bool DeleteAssignment(long assignmentId);
    /// <summary>
    /// Method updates the assignment if the data passed is valid and there is no conflict inside the data source
    /// </summary>
    /// <param name="assignment">assignment you want to update</param>
    /// <returns>True on success, false otherwise</returns>
    public bool UpdateAssignment(AssignmentPL assignment);
    /// <summary>
    /// Method returns all assignments that are under superiors power to control
    /// </summary>
    /// <param name="superiorId"></param>
    /// <returns>all assignments with wrapped project and user names</returns>
    public IEnumerable<AssignmentWrapperPayload>? GetAssignmentsUnderSuperior(long superiorId);
    //public bool UpdateProjectAssignmentsId(long projectId, long newId);
}