using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Shared.Models.ApiModels;

public class ProjectAssignPayload {
    public List<UserBase> AssignedUsers { get; set; }

    public long ProjectId { get; set; }


    public ProjectAssignPayload() {

    }

    public ProjectAssignPayload(List<UserBase> assigned, long projectId) {
        AssignedUsers = assigned;
        ProjectId = projectId;
    }
}