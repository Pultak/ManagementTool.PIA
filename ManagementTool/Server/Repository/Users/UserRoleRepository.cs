using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Models.Database;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Repository.Users;

public class UserRoleRepository : IUserRoleRepository {
    private readonly ManToolDbContext _db; //To Get all employees details
    private IMapper Mapper { get; }
    public UserRoleRepository(ManToolDbContext db, IMapper mapper){
        _db = db;
        Mapper = mapper;
    }


    public IEnumerable<RoleBLL> GetAllRoles() {
        var result = _db.Role?.ToList();
        return result == null ? Enumerable.Empty<RoleBLL>() : Mapper.Map<IEnumerable<RoleBLL>>(result);
    }

    public IEnumerable<RoleBLL> GetAllNonProjectManRoles() {
        var result = _db.Role?.Where(x => x.Type != ERoleType.ProjectManager);
        return result == null ? Enumerable.Empty<RoleBLL>() : Mapper.Map<IEnumerable<RoleBLL>>(result);
    }


    public IEnumerable<RoleBLL> GetRolesByType(ERoleType type) {
        var result = _db.Role?.Where(x => x.Type == type);
        return result == null ? Enumerable.Empty<RoleBLL>() : Mapper.Map<IEnumerable<RoleBLL>>(result);
    }

    public IEnumerable<RoleBLL> GetUserRolesByUserId(long userId) {
        if (_db.Role == null) {
            return Enumerable.Empty<RoleBLL>();
        }

        var queryResult = _db.UserRoleXRefs?.Join(_db.Role,
            refs => refs.IdRole,
            role => role.Id,
            (refs, role) => new{
                id = role.Id,
                roleName = role.Name,
                type = role.Type,
                projectId = role.ProjectId,
                userId = refs.IdUser
            }).Where(x=> x.userId == userId).
            Select(x => new RoleBLL(x.id, x.roleName, x.type, x.projectId)).ToList();
        return queryResult ?? Enumerable.Empty<RoleBLL>();
    }

    public long AddRole(RoleBLL role) {
        var dbRole = Mapper.Map<RoleDAL>(role);

        _db.Role?.Add(dbRole);
        var newRows = _db.SaveChanges();
        if (newRows <= 0) {
            return -1;
        }
        return dbRole.Id;
    }

    public bool DeleteProjectRole(long projectId) {
        var selectedRole = _db.Role?.SingleOrDefault(role => 
            role.Type == ERoleType.ProjectManager && role.ProjectId == projectId);
        if (selectedRole == null) {
            return false;
        }

        _db.Role?.Remove(selectedRole);
        var changedLines = _db.SaveChanges();
        return changedLines > 0;

    }

    public bool AssignRolesToUser(List<long> roles, long userId) {
        var resultRange = roles.Select(roleId => new UserRoleXRefsDAL {
            AssignedDate = DateTime.Now, 
            IdRole = roleId, 
            IdUser = userId
        }).ToList();

        _db.UserRoleXRefs?.AddRange(resultRange);
        var changedLines = _db.SaveChanges();
        return changedLines > 0;
    }

    public bool UnassignRolesFromUser(List<long> roles, long userId) {
        var userAssignments = _db.UserRoleXRefs?.Where(x => x.IdUser == userId && roles.Contains(x.IdRole));
        if (userAssignments == null) {
            return false;
        }

        _db.UserRoleXRefs?.RemoveRange(userAssignments);
        var changedLines = _db.SaveChanges();
        return changedLines > 0;
    }

    public bool UpdateProjectRoleName(long projectId, string roleName) {
        var selectedRole = _db.Role?.SingleOrDefault(role =>
            role.Type == ERoleType.ProjectManager && role.ProjectId == projectId);
        if (selectedRole == null) {
            return false;
        }
        selectedRole.Name = roleName;
        _db.Role?.Update(selectedRole);
        var changedLines = _db.SaveChanges();
        return changedLines > 0;

    }
}