using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Shared.Models.Api.Payloads;

/// <summary>
/// Wrapper for the assignment with project name and user name so it can be easily visualized in the view
/// </summary>
public class AssignmentWrapper
{

    /// <summary>
    /// Assignment and its variables to be visualized
    /// </summary>
    public Assignment Assignment { get; set; }
    /// <summary>
    /// Name of the project that this assignment is assigned to
    /// </summary>
    public string ProjectName { get; set; }

    /// <summary>
    /// Full name of the user that this assignment is assigned to 
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Creates wrapper with default values
    /// </summary>
    public AssignmentWrapper()
    {
        Assignment = new Assignment();
        ProjectName = "Unknown";
        UserName = "Unknown";
    }

    public AssignmentWrapper(string projectName, string userName, Assignment assignment)
    {
        Assignment = assignment;
        ProjectName = projectName;
        UserName = userName;
    }
}