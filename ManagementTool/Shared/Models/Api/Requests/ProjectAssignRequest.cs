using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Shared.Models.Api.Requests;

/// <summary>
/// Payload for the change of assigned users of desired project
/// </summary>
public class ProjectAssignPayload
{
    /// <summary>
    /// List of all assigned users to the selected project
    /// </summary>
    public List<UserBase> AssignedUsers { get; set; }

    /// <summary>
    /// Id of the project that we changed assigned users of
    /// </summary>
    public long ProjectId { get; set; }


    public ProjectAssignPayload()
    {

    }

    public ProjectAssignPayload(List<UserBase> assigned, long projectId)
    {
        AssignedUsers = assigned;
        ProjectId = projectId;
    }
}