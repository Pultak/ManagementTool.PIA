using System.ComponentModel.DataAnnotations;

namespace ManagementTool.Shared.Models.Presentation.Api.Requests; 

public class ProjectUpdateRequest {

    public ProjectPL Project { get; set; }

    public long ProjectManagerId { get; set; }

    public ProjectUpdateRequest() {
        Project = new ProjectPL();
    }
}