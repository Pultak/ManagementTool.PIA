namespace ManagementTool.Shared.Models.Presentation.Api.Requests;

/// <summary>
///     Payload for the change of assigned users of desired project
/// </summary>
public class ProjectAssignRequest {
    public ProjectAssignRequest() => AssignedUsers = new List<UserBasePL>();

    public ProjectAssignRequest(List<UserBasePL> assigned, long projectId) {
        AssignedUsers = assigned;
        ProjectId = projectId;
    }

    /// <summary>
    ///     List of all assigned users to the selected project
    /// </summary>
    public List<UserBasePL> AssignedUsers { get; set; }

    /// <summary>
    ///     Id of the project that we changed assigned users of
    /// </summary>
    public long ProjectId { get; set; }
}