using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ManagementTool.Server.Repository.Users;

public class UserRepository : IUserRepository {
    private readonly ManToolDbContext _db;

    public UserRepository(ManToolDbContext db, IMapper mapper) {
        _db = db;
        Mapper = mapper;
    }

    private IMapper Mapper { get; }

    public IEnumerable<UserBaseBLL> GetAllUsers() {
        var result = _db.User;

        return result == null ? Enumerable.Empty<UserBaseBLL>() : Mapper.Map<IEnumerable<UserBaseBLL>>(_db.User);
    }

    /// <summary>
    /// Adds new user to the data source with generated and hashed password and salt 
    /// </summary>
    /// <param name="user">new user model</param>
    /// <param name="pwd">generated hashed password</param>
    /// <param name="salt">salt for password hash</param>
    /// <returns>id of new user, -1 on failure</returns>
    public long AddUser(UserBaseBLL user, string pwd, string salt) {
        var newUser = Mapper.Map<UserDAL>(user);
        newUser.Pwd = pwd;
        newUser.PwdInit = true;
        newUser.Salt = salt;
        _db.User?.Add(newUser);
        if (_db.SaveChanges() <= 0) {
            return -1;
        }

        return newUser.Id;
    }

    /// <summary>
    /// Deletes the user from data source 
    /// </summary>
    /// <param name="userId">user to delete</param>
    /// <returns>true on success</returns>
    public bool DeleteUser(long userId) {
        var dbUser = _db.User?.Find(userId);
        if (dbUser == null) {
            return false;
        }

        _db.User?.Remove(dbUser);
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }

    /// <summary>
    /// Method retrieves user data by his id
    /// </summary>
    /// <param name="id">id of user</param>
    /// <returns></returns>
    public UserBaseBLL? GetUserById(long id) {
        var result = _db.User?.Find(id);

        return result == null ? null : Mapper.Map<UserBaseBLL>(result);
    }
    /// <summary>
    /// Method gets user data by his name
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public UserBaseBLL? GetUserByName(string username) {
        var result = _db.User?.SingleOrDefault(x => x.Username.Equals(username));
        return Mapper.Map<UserBaseBLL>(result);
    }

    public IEnumerable<UserBaseBLL> GetUsersById(IEnumerable<long> userIds) {
        var result = _db.User?.Where(x => userIds.Contains(x.Id)).Select(x =>
            new UserBaseBLL(x.Id, x.Username, x.FullName, x.PrimaryWorkplace, x.EmailAddress, x.PwdInit));
        return result ?? Enumerable.Empty<UserBaseBLL>();
    }
    /// <summary>
    /// Gets all credentials of user by his username (used during authorization)
    /// </summary>
    /// <param name="username">name of user for credentials</param>
    /// <returns>credentials</returns>
    public (long id, string pwd, string salt)? GetUserCredentials(string username) {
        var result = _db.User?.SingleOrDefault(user => string.Equals(user.Username, username));

        return result == null ? null : (result.Id, result.Pwd, result.Salt);
    }
    /// <summary>
    /// Gets all credentials of user by his id (used during authorization)
    /// </summary>
    /// <param name="username">name of user for credentials</param>
    /// <returns>credentials</returns>
    public (long id, string pwd, string salt)? GetUserCredentials(long id) {
        var result = _db.User?.Find(id);
        return result == null ? null : (result.Id, result.Pwd, result.Salt);
    }

    /// <summary>
    /// Updates the user data 
    /// </summary>
    /// <param name="user"></param>
    /// <returns>true on success</returns>
    public bool UpdateUser(UserBaseBLL user) {
        var dbUser = Mapper.Map<UserDAL>(user);
        _db.Entry(dbUser).State = EntityState.Modified;
        //don't update pwd and username
        var entry = _db.Entry(dbUser);

        entry.Property(o => o.Pwd).IsModified = false;
        entry.Property(o => o.Username).IsModified = false;
        entry.Property(o => o.Salt).IsModified = false;
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }

    public bool UpdateUserPwd(long userId, string newPwd) {
        var dbUser = _db.User?.Find(userId);
        if (dbUser == null) {
            return false;
        }

        var entry = _db.Entry(dbUser);
        dbUser.Pwd = newPwd;
        dbUser.PwdInit = false;

        //update only pwd and username
        entry.Property(o => o.Pwd).IsModified = true;
        entry.Property(o => o.PwdInit).IsModified = true;
        /*entry.Property(o => o.Username).IsModified = false;
        entry.Property(o => o.PrimaryWorkplace).IsModified = false;
        entry.Property(o => o.EmailAddress).IsModified = false;
        entry.Property(o => o.FullName).IsModified = false;*/
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }

    /// <summary>
    /// Method retrieves all users with specified role
    /// </summary>
    /// <param name="roleId">id of role you want to retrieve</param>
    /// <returns>List of all users with role</returns>
    public IEnumerable<UserBaseBLL> GetAllUsersByRole(long roleId) {
        if (_db.User == null) {
            return Enumerable.Empty<UserBaseBLL>();
        }

        var queryResult = _db.UserRoleXRefs?.Join(_db.User,
            refs => refs.IdUser,
            user => user.Id,
            (refs, user) => new {
                id = user.Id,
                username = user.Username,
                pwd = user.Pwd,
                fullName = user.FullName,
                primaryWorkplace = user.PrimaryWorkplace,
                emailAddress = user.EmailAddress,
                salt = user.Salt,
                pwdInit = user.PwdInit,
                roleId = refs.IdRole
            }).Where(x => x.roleId == roleId).Select(x => new UserBaseBLL(x.id, x.username, x.fullName,
            x.primaryWorkplace,
            x.emailAddress, x.pwdInit));

        return queryResult ?? Enumerable.Empty<UserBaseBLL>();
    }

    /// <summary>
    /// Method retrieves all users that are superior to specified user
    /// </summary>
    /// <param name="userId">id of user you want superiors of</param>
    /// <returns>list of all ids</returns>
    public IEnumerable<long> GetAllUserSuperiorsIds(long userId) {
        var queryResult = _db.UserSuperiorXRefs?.Where(x => x.IdUser == userId).Select(x => x.IdSuperior);
        return queryResult ?? Enumerable.Empty<long>();
    }

    /// <summary>
    /// Method retrieves all users and add a flag if they are assigned to it or not
    /// </summary>
    /// <param name="projectId">id of project the user should be assigned to</param>
    /// <returns>List of all users with flags</returns>
    public IEnumerable<DataModelAssignmentBLL<UserBaseBLL>> GetAllUsersAssignedToProjectWrappers(long projectId) {
        if (_db.User == null || _db.UserProjectXRefs == null) {
            return Array.Empty<DataModelAssignmentBLL<UserBaseBLL>>();
        }
        var query = (from user in _db.User
            from refs in _db.UserProjectXRefs.Where(x => x.IdProject == projectId)
            select new DataModelAssignmentBLL<UserBaseBLL>(refs != null,
                new UserBaseBLL(user.Id, user.Username, user.FullName,
                    user.PrimaryWorkplace, user.EmailAddress, user.PwdInit))).Distinct().ToList();

        return query;
    }


    /// <summary>
    /// Method retrieves all users and add a flag if they are assigned to it or not
    /// </summary>
    /// <param name="projectId">id of project the user should be assigned to</param>
    /// <returns>List of all users with flags</returns>
    public IEnumerable<UserBaseBLL> GetAllUsersAssignedToProject(long projectId) {
        if (_db.User == null || _db.UserProjectXRefs == null) {
            return Array.Empty<UserBaseBLL>();
        }

        var query = _db.User.Join(_db.UserProjectXRefs,
                user => user.Id,
                refs => refs.IdUser,
                (user, refs) => new {
                    idUser = user.Id,
                    username = user.Username,
                    fullName = user.FullName,
                    primaryWorklplace = user.PrimaryWorkplace,
                    emailAddress = user.EmailAddress,
                    pwdInit = user.PwdInit,
                    projectId = refs.IdProject
                }).Where(x => x.projectId == projectId)
            .Select(x =>
                new UserBaseBLL(x.idUser, x.username, x.fullName, x.primaryWorklplace, x.emailAddress, x.pwdInit));

        return query;
    }


    /// <summary>
    /// Creates a user/superior assignation
    /// </summary>
    /// <param name="superiorsIds"></param>
    /// <param name="userId"></param>
    /// <returns>true on success</returns>
    public bool AssignSuperiorsToUser(List<long> superiorsIds, long userId) {
        List<UserSuperiorXRefsDAL> resultRange = new();
        foreach (var superiorId in superiorsIds) {
            var newRefs = new UserSuperiorXRefsDAL {
                IdUser = userId,
                IdSuperior = superiorId,
                AssignedDate = DateTime.Now
            };
            resultRange.Add(newRefs);
        }

        _db.UserSuperiorXRefs?.AddRange(resultRange);

        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }
    /// <summary>
    /// Removes all superior assignations to the project
    /// </summary>
    /// <param name="superiorsIds"></param>
    /// <param name="userId"></param>
    /// <returns>true on success</returns>
    public bool UnassignSuperiorsFromUser(List<long> superiorsIds, long userId) {
        var result = _db.UserSuperiorXRefs?.Where(
            o => o.IdUser == userId && superiorsIds.Contains(o.IdSuperior));
        if (result == null || !result.Any()) {
            return false;
        }

        _db.RemoveRange(result);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }

    /*public IEnumerable<UserBaseBLL> GetAllUsersUnderProject(long projectId) {
        if (_db.User == null) {
            return Enumerable.Empty<UserBaseBLL>();
        }

        var query = _db.UserProjectXRefs?.Where(x => x.IdProject == projectId).Join(_db.User,
            refs => refs.IdUser,
            user => user.Id,
            (refs, user) => user);


        return query == null ? Enumerable.Empty<UserBaseBLL>() : Mapper.Map<UserBaseBLL[]>(query);
    }*/

    /// <summary>
    /// Creates a user/project assignation
    /// </summary>
    /// <param name="usersIds">users that should be part of project</param>
    /// <param name="projectId">project</param>
    /// <returns>true on success</returns>
    public bool AssignUsersToProject(List<long> usersIds, long projectId) {
        List<UserProjectXRefsDAL> resultRange = new();
        foreach (var userId in usersIds) {
            var newRefs = new UserProjectXRefsDAL {
                IdUser = userId,
                IdProject = projectId,
                AssignedDate = DateTime.Now,
                Id = default
            };
            resultRange.Add(newRefs);
        }

        _db.UserProjectXRefs?.AddRange(resultRange);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }

    /// <summary>
    /// Removes all user assignations to the project
    /// </summary>
    /// <param name="usersIds"></param>
    /// <param name="projectId"></param>
    /// <returns>true on success</returns>
    public bool UnassignUsersFromProject(List<long> usersIds, long projectId) {
        var result = _db.UserProjectXRefs?.Where(
            o => usersIds.Contains(o.IdUser) && o.IdProject == projectId);
        if (result == null || !result.Any()) {
            return false;
        }

        _db.RemoveRange(result);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }


    public bool IsUserAssignedToProject(long userId, long projectId) {
        var reference = _db.UserProjectXRefs?.SingleOrDefault(
            refs => refs.IdProject == projectId && refs.IdUser == userId
        );

        return reference != null;
    }


    public UserBaseBLL? GetProjectManagerByRoleId(long roleId) {
        if (_db.User == null) {
            return null;
        }

        var result = _db.UserRoleXRefs?.Where(x => x.IdRole == roleId).Join(_db.User,
            assign => assign.IdUser,
            user => user.Id,
            (assign, user) => new UserBaseBLL {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                PrimaryWorkplace = user.PrimaryWorkplace,
                EmailAddress = user.EmailAddress,
                PwdInit = user.PwdInit
            }).FirstOrDefault();

        return result;
    }
    
}