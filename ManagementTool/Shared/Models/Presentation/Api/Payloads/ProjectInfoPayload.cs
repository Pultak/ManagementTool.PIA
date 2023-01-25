using System.ComponentModel.DataAnnotations;
using ManagementTool.Shared.Models.Presentation.Api.Requests;

namespace ManagementTool.Shared.Models.Presentation.Api.Payloads;

/// <summary>
///     Payload used upon receiving project data from the server. It also contains the name and id of the assigned project
///     manager
/// </summary>
public class ProjectInfoPayload : ProjectUpdateRequest {
    public ProjectInfoPayload() => ManagerName = string.Empty;

    /// <summary>
    ///     Manager name that has this project assigned
    /// </summary>
    public string ManagerName { get; set; }
}