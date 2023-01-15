using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Shared.Models.ApiModels; 

public class AssignmentWrapper {

    public Assignment Assignment { get; set; }
    public string ProjectName { get; set; }
    public string UserName { get; set; }

    public AssignmentWrapper() {
        Assignment = new Assignment();
        ProjectName = "Unknown";
        UserName = "Unknown";
    }

    public AssignmentWrapper(string projectName, string userName, Assignment assignment) {
        Assignment = assignment;
        ProjectName = projectName;
        UserName = userName;
    }
}