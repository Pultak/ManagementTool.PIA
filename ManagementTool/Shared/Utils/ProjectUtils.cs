using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Shared.Utils; 

public static class ProjectUtils {
    public static readonly DateTime RefDateTime = new(2010, 1, 1);


    public static EProjectCreationResponse ValidateNewProject(Project? project) {
        if(project == null) {
            return EProjectCreationResponse.EmptyProject;
        }

        if (project.ProjectName.Length is < 2 or > 80) {
            return EProjectCreationResponse.InvalidName;
        }

        var timeDiff = DateTime.Compare(RefDateTime, project.FromDate);
        if (timeDiff > 0) {
            //earlier from date than allowed!
            return EProjectCreationResponse.InvalidFromDate;
        }

        if (project.ToDate != null) {
            timeDiff = DateTime.Compare(project.FromDate, project.ToDate.Value);
            if (timeDiff >= 0) {
                //to date is earlier or from the same time
                return EProjectCreationResponse.InvalidToDate;
            }
        }

        if (project.Description.Length > 1024) {
            return EProjectCreationResponse.InvalidDescription;
        }

        return EProjectCreationResponse.Ok;
    }
}