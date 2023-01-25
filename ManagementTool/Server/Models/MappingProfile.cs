using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Models.Database;
using ManagementTool.Shared.Models.Presentation;

namespace ManagementTool.Server.Models;

/// <summary>
/// Class used for mapping of models between layers
/// </summary>
internal class MappingProfile : Profile {
    public MappingProfile() {
        //map init of user models
        CreateMap<UserDAL, UserBaseBLL>();
        CreateMap<UserBaseBLL, UserDAL>();
        CreateMap<UserBasePL, UserBaseBLL>();
        CreateMap<UserBaseBLL, UserBasePL>();

        //map init of project models
        CreateMap<ProjectDAL, ProjectBLL>();
        CreateMap<ProjectBLL, ProjectDAL>();
        CreateMap<ProjectPL, ProjectBLL>();
        CreateMap<ProjectBLL, ProjectPL>();

        //map init of assignment models
        CreateMap<AssignmentDAL, AssignmentBLL>();
        CreateMap<AssignmentBLL, AssignmentDAL>();
        CreateMap<AssignmentPL, AssignmentBLL>();
        CreateMap<AssignmentBLL, AssignmentPL>();

        //map init of role models
        CreateMap<RoleDAL, RoleBLL>();
        CreateMap<RoleBLL, RoleDAL>();
        CreateMap<RolePL, RoleBLL>();
        CreateMap<RoleBLL, RolePL>();
    }
}