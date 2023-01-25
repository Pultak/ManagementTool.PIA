using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Models.Database;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Repository.Users;

public class UserRoleRepository : IUserRoleRepository {
    private readonly ManToolDbContext _db; //To Get all employees details

    public UserRoleRepository(ManToolDbContext db, IMapper mapper) {
        _db = db;
        Mapper = mapper;
    }

    private IMapper Mapper { get; }


    /// <summary>
    /// Data source returns all roles that are not of project manager type
    /// </summary>
    /// <returns></returns>
    public IEnumerable<RoleBLL> GetAllNonProjectManRoles() {
        var result = _db.Role?.Where(x => x.Type != RoleType.ProjectManager);
        return result == null ? Enumerable.Empty<RoleBLL>() : Mapper.Map<IEnumerable<RoleBLL>>(result);
    }

    /// <summary>
    /// Returns all roles that are of specified type
    /// </summary>
    /// <param name="type">specified role type</param>
    /// <returns></returns>
    public IEnumerable<RoleBLL> GetRolesByType(RoleType type) {
        var result = _db.Role?.Where(x => x.Type == type);
        return result == null ? Enumerable.Empty<RoleBLL>() : Mapper.Map<IEnumerable<RoleBLL>>(result);
    }
    /// <summary>
    /// Data source returns all user roles that are assigned to user
    /// </summary>
    /// <param name="userId">user that holds the roles</param>
    /// <returns></returns>
    public IEnumerable<RoleBLL> GetUserRolesByUserId(long userId) {
        if (_db.Role == null) {
            return Enumerable.Empty<RoleBLL>();
        }

        var queryResult = _db.UserRoleXRefs?.Join(_db.Role,
            refs => refs.IdRole,
            role => role.Id,
            (refs, role) => new {
                id = role.Id,
                roleName = role.Name,
                type = role.Type,
                projectId = role.ProjectId,
                userId = refs.IdUser
            }).Where(x => x.userId == userId).Select(x => new RoleBLL(x.id, x.roleName, x.type, x.projectId)).ToList();
        return queryResult ?? Enumerable.Empty<RoleBLL>();
    }
    /// <summary>
    /// Creates new role inside of data source
    /// </summary>
    /// <param name="role">New role object</param>
    /// <returns>id of new role, -1 on failure</returns>
    public long AddRole(RoleBLL role) {
        var dbRole = Mapper.Map<RoleDAL>(role);
        _db.Role?.Add(dbRole);
        var newRows = _db.SaveChanges();
        if (newRows <= 0) {
            return -1;
        }

        return dbRole.Id;
    }
    /// <summary>
    /// Deletes the project manager role (used on project deletion)
    /// </summary>
    /// <param name="projectId">id of project and project manager role</param>
    /// <returns>true on success</returns>
    public bool DeleteProjectRole(long projectId) {
        var selectedRole = _db.Role?.SingleOrDefault(role =>
            role.Type == RoleType.ProjectManager && role.ProjectId == projectId);
        if (selectedRole == null) {
            return false;
        }

        _db.Role?.Remove(selectedRole);
        var changedLines = _db.SaveChanges();
        return changedLines > 0;
    }
    /// <summary>
    /// Method assigns all roles to specified user
    /// </summary>
    /// <param name="roles">ids of all existing roles </param>
    /// <param name="userId">user that will have new roles assigned</param>
    /// <returns>true on success</returns>
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
    /// <summary>
    /// Method removes all roles from specified user
    /// </summary>
    /// <param name="roles">ids of all existing roles </param>
    /// <param name="userId">user that will have new roles removed</param>
    /// <returns>true on success</returns>
    public bool UnassignRolesFromUser(List<long> roles, long userId) {
        var userAssignments = _db.UserRoleXRefs?.Where(x => x.IdUser == userId && roles.Contains(x.IdRole));
        if (userAssignments == null) {
            return false;
        }

        _db.UserRoleXRefs?.RemoveRange(userAssignments);
        var changedLines = _db.SaveChanges();
        return changedLines > 0;
    }

    /// <summary>
    /// Updates the project manager role name 
    /// </summary>
    /// <param name="projectId">id of project and project manager role</param>
    /// <param name="roleName">new name</param>
    /// <returns>true on success</returns>
    public bool UpdateProjectRoleName(long projectId, string roleName) {
        var selectedRole = _db.Role?.SingleOrDefault(role =>
            role.Type == RoleType.ProjectManager && role.ProjectId == projectId);
        if (selectedRole == null) {
            return false;
        }

        selectedRole.Name = roleName;
        _db.Role?.Update(selectedRole);
        var changedLines = _db.SaveChanges();
        return changedLines > 0;
    }
    /// <summary>
    /// Method changes the project manager user of specified project
    /// </summary>
    /// <param name="projectId"> project that should have new project manager </param>
    /// <param name="userId">new project manager</param>
    /// <returns>true on success</returns>
    public bool UpdateProjectManager(long projectId, long userId) {
        
        var selectedRole = _db.Role?.SingleOrDefault(role =>
            role.Type == RoleType.ProjectManager && role.ProjectId == projectId);
        if (selectedRole == null) {
            return false;
        }
        
        var assign = _db.UserRoleXRefs?.SingleOrDefault(x => x.IdRole == selectedRole.Id);
        
        if (assign == null) {
            //no assign existent atm
            UserRoleXRefsDAL newAssign = new() {
                AssignedDate = DateTime.Now,
                IdRole = selectedRole.Id,
                IdUser = userId
            };
            _db.UserRoleXRefs?.Add(newAssign);
        }
        else {
            assign.IdUser = userId;
            _db.UserRoleXRefs?.Update(assign);

        }

        var changedLines = _db.SaveChanges();
        return changedLines > 0;
    }
    /// <summary>
    /// Gets the project manager role by project id
    /// </summary>
    /// <param name="projectId">id of project you want role of</param>
    /// <returns>null if there is no such project role</returns>
    public RoleBLL? GetRoleByProjectId(long projectId) {

        var result = _db.Role?.SingleOrDefault(x => x.ProjectId == projectId);

        return result == null ? null : Mapper.Map<RoleBLL>(result);
    }
    
}