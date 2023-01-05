using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Server.Services.Users;

public class UserRoleDataService : IUserRoleDataService {
    private readonly ManToolDbContext _db; //To Get all employees details
    public UserRoleDataService(ManToolDbContext db){
        _db = db;
    }


    public IEnumerable<Role> GetAllRoles() {
        return _db.Role.ToList();
    }

    public IEnumerable<Role> GetUserRolesById(long userId) {
        var queryResult = _db.UserRoleXRefs.Join(_db.Role,
            refs => refs.IdRole,
            role => role.Id,
            (refs, role) => new{
                id = role.Id,
                roleName = role.Name,
                type = role.Type,
                projectId = role.ProjectId,
                userId = refs.IdUser
            }).Where(x=> x.userId == userId).
            Select(x => new Role(x.id, x.roleName, x.type, x.projectId)).ToList();
        return queryResult;
    }

    public long AddRole(Role role) {
        _db.Role.Add(role);
        var newRows = _db.SaveChanges();
        if (newRows <= 0) {
            return -1;
        }
        return role.Id;
    }
}