using AutoMapper;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;

namespace ManagementTool.Server.Models.Business;

/// <summary>
///     Wrapper for the assignment with project name and user name so it can be easily visualized in the view
/// </summary>
public class AssignmentWrapperBLL {
    /// <summary>
    ///     Creates wrapper with default values
    /// </summary>
    public AssignmentWrapperBLL() {
        Assignment = new AssignmentBLL();
        ProjectName = "Unknown";
        UserName = "Unknown";
    }

    public AssignmentWrapperBLL(string projectName, string userName, AssignmentBLL assignment) {
        Assignment = assignment;
        ProjectName = projectName;
        UserName = userName;
    }

    /// <summary>
    ///     Assignment and its variables to be visualized
    /// </summary>
    public AssignmentBLL Assignment { get; set; }

    /// <summary>
    ///     Name of the project that this assignment is assigned to
    /// </summary>
    public string ProjectName { get; set; }

    /// <summary>
    ///     Full name of the user that this assignment is assigned to
    /// </summary>
    public string UserName { get; set; }

    public AssignmentWrapperPayload MapToPL(IMapper mapper) {
        var obj = mapper.Map<AssignmentPL>(Assignment);

        return new AssignmentWrapperPayload(ProjectName, UserName, obj);
    }

    public AssignmentWrapperBLL MapToBLL(AssignmentWrapperPayload wrapper, IMapper mapper) {
        var obj = mapper.Map<AssignmentBLL>(wrapper.Assignment);

        return new AssignmentWrapperBLL(wrapper.ProjectName, wrapper.UserName, obj);
    }
}