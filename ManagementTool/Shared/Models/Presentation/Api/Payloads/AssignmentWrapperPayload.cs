namespace ManagementTool.Shared.Models.Presentation.Api.Payloads;

/// <summary>
///     Wrapper for the assignment with project name and user name so it can be easily visualized in the view
/// </summary>
public class AssignmentWrapperPayload {
    /// <summary>
    ///     Creates wrapper with default values
    /// </summary>
    public AssignmentWrapperPayload() {
        Assignment = new AssignmentPL();
        ProjectName = "Unknown";
        UserName = "Unknown";
    }

    public AssignmentWrapperPayload(string projectName, string userName, AssignmentPL assignment) {
        Assignment = assignment;
        ProjectName = projectName;
        UserName = userName;
    }

    /// <summary>
    ///     Assignment and its variables to be visualized
    /// </summary>
    public AssignmentPL Assignment { get; set; }

    /// <summary>
    ///     Name of the project that this assignment is assigned to
    /// </summary>
    public string ProjectName { get; set; }

    /// <summary>
    ///     Full name of the user that this assignment is assigned to
    /// </summary>
    public string UserName { get; set; }
}