using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Models.Database;
using ManagementTool.Shared.Models.Presentation;

namespace ManagementTool.Server.Models;

internal class MappingProfile : Profile {

    public MappingProfile() {
        CreateMap<UserDAL, UserBaseBLL>();
        CreateMap<UserBaseBLL, UserDAL>();

        CreateMap<ProjectDAL, ProjectBLL>();
        CreateMap<ProjectBLL, ProjectDAL>();

        CreateMap<AssignmentDAL, AssignmentBLL>();
        CreateMap<AssignmentBLL, AssignmentDAL>();

        CreateMap<RoleDAL, RoleBLL>();
        CreateMap<RoleBLL, RoleDAL>();
    }


}