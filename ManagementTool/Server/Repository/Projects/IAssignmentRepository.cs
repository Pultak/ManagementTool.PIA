using ManagementTool.Server.Models.Business;

namespace ManagementTool.Server.Repository.Projects;

public interface IAssignmentRepository {
    /*public AssignmentBLL? GetAssignmentByName(string name);
    public AssignmentBLL? GetAssignmentById(long id);
    public IEnumerable<AssignmentBLL> GetAllPlainAssignments();
    */
    
    /// <summary>
    /// Retrieves all assignments with user name and project name wrapped
    /// </summary>
    /// <returns></returns>
    public IEnumerable<AssignmentWrapperBLL> GetAllAssignments();
    /// <summary>
    /// Retrieves all assignments assigned to user with user name and project name wrapped
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IEnumerable<AssignmentWrapperBLL> GetAssignmentsByUserId(long userId);
    /// <summary>
    /// Retrieves all assignments that are under specified projects with user name and project name wrapped
    /// </summary>
    /// <param name="projectIds">list of all assignments</param>
    /// <returns></returns>
    public IEnumerable<AssignmentWrapperBLL> GetAssignmentsByProjectIds(IEnumerable<long> projectIds);
    //public IEnumerable<AssignmentBLL> GetAssignmentsByProjectId(long projectId);

    /// <summary>
    /// method updates assignments mainly called upon project deletion 
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="newProjectId"></param>
    /// <returns></returns>
    public bool UpdateAssignmentProjectIds(long projectId, long newProjectId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="superiorId"></param>
    /// <returns></returns>
    public IEnumerable<AssignmentWrapperBLL> GetAssignmentsUnderSuperior(long superiorId);
    
    /// <summary>
    /// Add new assignment to the data source
    /// </summary>
    /// <param name="assignment">new assignment</param>
    /// <returns>id of new assignment, -1 on failure</returns>
    public long AddAssignment(AssignmentBLL assignment);

    /// <summary>
    /// Updates the assignment data inside of data source
    /// </summary>
    /// <param name="assignment"></param>
    /// <returns>true on success</returns>
    public bool UpdateAssignment(AssignmentBLL assignment);
    //public bool UpdateAssignments(IEnumerable<AssignmentBLL> assignments);

    /// <summary>
    /// Method return all assignments assigned to specified users
    /// </summary>
    /// <param name="userIds">specified users</param>
    /// <returns>list of all assignments</returns>
    public IEnumerable<AssignmentBLL> GetAllUsersAssignments(long[] userIds);
    
    /// <summary>
    /// Deletes specified assignment but must be already existing inside of the data source
    /// </summary>
    /// <param name="id">id of the assignment to delete</param>
    /// <returns>true on success</returns>
    public bool DeleteAssignment(long id);
}