using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ManagementTool.Server.Repository.Users;

public class UserRepository : IUserRepository{
    private readonly ManToolDbContext _db; 
    private IMapper Mapper { get; }
    public UserRepository(ManToolDbContext db, IMapper mapper){
        _db = db;
        Mapper = mapper;
    }

    public IEnumerable<UserBaseBLL> GetAllUsers() {
        var result = _db.User;
        
        return result == null ? Enumerable.Empty<UserBaseBLL>() : Mapper.Map<UserBaseBLL[]>(_db.User);
    }


    public long AddUser(UserBaseBLL user, string pwd, string salt) {
        var newUser = Mapper.Map<UserDAL>(user);
        newUser.Pwd = pwd;
        newUser.PwdInit = true;
        newUser.Salt = salt;
        _db.User?.Add(newUser);
        if (_db.SaveChanges() <= 0) {
            return -1;
        }
        return user.Id;
    }

    public bool DeleteUser(long userId) {
        var dbUser = _db.User?.Find(userId);
        if (dbUser == null) {
            return false;
        }
        _db.User?.Remove(dbUser);
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }

    public UserBaseBLL? GetUserById(long id) {
        var result = _db.User?.Find(id);

        return result == null ? null : Mapper.Map<UserBaseBLL>(result);
    }

    public IEnumerable<UserBaseBLL> GetUsersById(IEnumerable<long> userIds) {
        var result = _db.User?.Where(x => userIds.Contains(x.Id)).
            Select(x => new UserBaseBLL(x.Id, x.Username, x.FullName, x.PrimaryWorkplace, x.EmailAddress, x.PwdInit));
        return result ?? Enumerable.Empty<UserBaseBLL>();
    }

    public (long id, string pwd, string salt)? GetUserPassword(string username) {
        var result = _db.User?.SingleOrDefault(user => string.Equals(user.Username, username));

        return result == null ? null : (result.Id, result.Pwd, result.Salt);
    }
    
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
                }).Where(x => x.roleId == roleId).
            Select(x => new UserBaseBLL(x.id, x.username, x.fullName, x.primaryWorkplace,
                x.emailAddress, x.pwdInit));

        return queryResult ?? Enumerable.Empty<UserBaseBLL>();
    }

     public IEnumerable<long> GetAllUserSuperiorsIds(long userId) {
         var queryResult = _db.UserSuperiorXRefs?.Where(x => x.IdUser == userId).Select(x => x.IdSuperior);
        return queryResult ?? Enumerable.Empty<long>();
    }



    public IEnumerable<DataModelAssignmentBLL<UserBaseBLL>> GetAllUsersAssignedToProject(long projectId) {
        var query = (from user in _db.User
            from refs in _db.UserProjectXRefs.Where(x => x.IdProject == projectId).DefaultIfEmpty()
            select new DataModelAssignmentBLL<UserBaseBLL>(refs != null, 
                new UserBaseBLL(user.Id, user.Username, user.FullName,
                user.PrimaryWorkplace, user.EmailAddress, user.PwdInit))).Distinct().ToList();
        
        return query;
    }


    public bool AssignSuperiorsToUser(List<long> superiorsIds, long userId) {
        List<UserSuperiorXRefsDAL> resultRange = new();
        foreach (var superiorId in superiorsIds) {
            var newRefs = new UserSuperiorXRefsDAL{
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

    public bool UnassignSuperiorsFromUser(List<long> superiorsIds, long userId){
        var result = _db.UserSuperiorXRefs?.Where(
            o => o.IdUser == userId && superiorsIds.Contains(o.IdSuperior));
        if (result == null || !result.Any()) {
            return false;
        }
        _db.RemoveRange(result);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }

    public IEnumerable<UserBaseBLL> GetAllUsersUnderProject(long projectId) {
        if (_db.User == null) {
            return Enumerable.Empty<UserBaseBLL>();
        }
        var query = _db.UserProjectXRefs?.Where(x => x.IdProject == projectId).Join(_db.User,
            refs => refs.IdUser,
            user => user.Id,
            (refs, user) => user);


        return query == null ? Enumerable.Empty<UserBaseBLL>() : Mapper.Map<UserBaseBLL[]>(query);
    }
    

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
}