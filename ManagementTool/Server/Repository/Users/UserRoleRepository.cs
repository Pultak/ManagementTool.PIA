using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Users;

public class UserRoleRepository : IUserRoleRepository {
    private readonly ManToolDbContext _db; //To Get all employees details
    public UserRoleRepository(ManToolDbContext db){
        _db = db;
    }


    public IEnumerable<Role> GetAllRoles() {
        return _db.Role.ToList();
    }

    public IEnumerable<Role> GetAllNonProjectManRoles() {
        return _db.Role.Where(x => x.Type != ERoleType.ProjectManager);
    }


    public IEnumerable<Role> GetRolesByType(ERoleType type) {
        return _db.Role.Where(x => x.Type == type);
    }

    public IEnumerable<Role> GetUserRolesByUserId(long userId) {
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

    public bool DeleteProjectRole(long projectId) {
        var selectedRole = _db.Role.SingleOrDefault(role => 
            role.Type == ERoleType.ProjectManager && role.ProjectId == projectId);
        if (selectedRole == null) {
            return false;
        }

        _db.Role.Remove(selectedRole);
        var changedLines = _db.SaveChanges();
        return changedLines > 0;

    }

    public bool AssignRolesToUser(List<Role> roles, long userId) {
        List<UserRoleXRefs> resultRange = new List<UserRoleXRefs>();
        foreach (var role in roles) {

            UserRoleXRefs newAssignment = new() {
                AssignedDate = DateTime.Now,
                IdRole = role.Id,
                IdUser = userId
            };
            resultRange.Add(newAssignment);
        }

        _db.UserRoleXRefs.AddRange(resultRange);
        var changedLines = _db.SaveChanges();
        return changedLines > 0;
    }

    public bool UnassignRolesFromUser(List<Role> roles, long userId) {
        var userAssignments = _db.UserRoleXRefs.Where(x => x.IdUser == userId);
        foreach (var roleRef in userAssignments) {
            if (roles.Any(role => role.Id == roleRef.IdRole)) {
                _db.UserRoleXRefs.Remove(roleRef);
            }
        }
        var changedLines = _db.SaveChanges();
        return changedLines > 0;

    }

    public bool UpdateProjectRoleName(long projectId, string roleName) {
        var selectedRole = _db.Role.SingleOrDefault(role =>
            role.Type == ERoleType.ProjectManager && role.ProjectId == projectId);
        if (selectedRole == null) {
            return false;
        }
        selectedRole.Name = roleName;
        _db.Role.Update(selectedRole);
        var changedLines = _db.SaveChanges();
        return changedLines > 0;

    }
}