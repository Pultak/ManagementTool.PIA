using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Shared.Utils;

public static class ProjectUtils {
    /// <summary>
    ///     Validation constants for checking project name length
    /// </summary>
    public const int MinProjectNameLength = 2;

    public const int MaxProjectNameLength = 80;

    /// <summary>
    ///     Validation constants for checking project name length
    /// </summary>
    public const int MinProjectDescriptionLength = 2;

    public const int MaxProjectDescriptionLength = 2048;


    /// <summary>
    ///     Oldest possible datetime that the new dates need to follow
    /// </summary>
    public static readonly DateTime RefDateTime = new(2010, 1, 1);


    /// <summary>
    ///     Validates the passed project if it contains all required variables of desired quality
    /// </summary>
    /// <param name="project">project you want check</param>
    /// <returns>Ok if valid, otherwise EProjectCreationResponse with error</returns>
    public static ProjectCreationResponse ValidateNewProject(ProjectPL? project) {
        if (project == null) {
            return ProjectCreationResponse.EmptyProject;
        }

        if (project.ProjectName.Length is < MinProjectNameLength or > MaxProjectNameLength) {
            return ProjectCreationResponse.InvalidName;
        }

        var timeDiff = DateTime.Compare(RefDateTime, project.FromDate);
        if (timeDiff > 0) {
            //earlier from date than allowed!
            return ProjectCreationResponse.InvalidFromDate;
        }

        if (project.ToDate != null) {
            timeDiff = DateTime.Compare(project.FromDate, project.ToDate.Value);
            if (timeDiff >= 0) {
                //to date is earlier or from the same time
                return ProjectCreationResponse.InvalidToDate;
            }
        }

        if (project.Description.Length > 1024) {
            return ProjectCreationResponse.InvalidDescription;
        }

        return ProjectCreationResponse.Ok;
    }
}