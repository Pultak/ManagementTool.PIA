using ManagementTool.Server.Services;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Api.Payloads;
using ManagementTool.Shared.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ManagementTool.Server.Repository.Users;

public class UserRepository : IUserRepository{
    private readonly ManToolDbContext _db; 
    public UserRepository(ManToolDbContext db){
        _db = db;
    }

    public IEnumerable<UserBase> GetAllUsers() {
        return _db.User.ToList();
    }



    public long AddUser(User user) {
        _db.User.Add(user);
        if (_db.SaveChanges() <= 0) {
            return -1;
        }
        return user.Id;
    }

    public bool DeleteUser(User user) {
        //_db.User.Attach(dummyUser);
        _db.User.Remove(user);
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }

    public User? GetUserById(long id) {
        return _db.User.Find(id);
    }

    public IEnumerable<UserBase> GetUsersById(IEnumerable<long> userIds) {
        var result = _db.User.Where(x => userIds.Contains(x.Id)).
            Select(x => new UserBase(x.Id, x.Username, x.FullName, x.PrimaryWorkplace, x.EmailAddress, x.PwdInit));
        return result;
    }

    public User? GetUserByName(string username) {
        return _db.User.SingleOrDefault(user => string.Equals(user.Username, username));
    }
    
    public bool UpdateUser(User user) {
        _db.Entry(user).State = EntityState.Modified;
        //don't update pwd and username
        _db.Entry(user).Property(o => o.Pwd).IsModified = false;
        _db.Entry(user).Property(o => o.Username).IsModified = false;
        _db.Entry(user).Property(o => o.Salt).IsModified = false;
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }

    public bool UpdateUserPwd(User user) {
        _db.Entry(user).State = EntityState.Modified;
        //don't update pwd and username
        _db.Entry(user).Property(o => o.Pwd).IsModified = true;
        _db.Entry(user).Property(o => o.PwdInit).IsModified = true;
        _db.Entry(user).Property(o => o.Username).IsModified = false;
        _db.Entry(user).Property(o => o.PrimaryWorkplace).IsModified = false;
        _db.Entry(user).Property(o => o.EmailAddress).IsModified = false;
        _db.Entry(user).Property(o => o.FullName).IsModified = false;
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }

    public IEnumerable<UserBase> GetAllUsersByRole(Role role) {
        var queryResult = _db.UserRoleXRefs.Join(_db.User,
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
                }).Where(x => x.roleId == role.Id).
            Select(x => new User(x.id, x.username, x.fullName, x.primaryWorkplace,
                x.emailAddress, x.pwd, x.salt, x.pwdInit));
        return queryResult;
    }

     public IEnumerable<long> GetAllUserSuperiorsIds(long userId) {
         var queryResult = _db.UserSuperiorXRefs.Where(x => x.IdUser == userId).Select(x => x.IdSuperior);
        return queryResult;
    }



    public IEnumerable<DataModelAssignment<UserBase>> GetAllUsersAssignedToProject(long projectId) {
        var query = (from user in _db.User
            from refs in _db.UserProjectXRefs.Where(x => x.IdProject == projectId).DefaultIfEmpty()
            select new DataModelAssignment<UserBase>(refs != null, 
                new UserBase(user.Id, user.Username, user.FullName,
                user.PrimaryWorkplace, user.EmailAddress, user.PwdInit))).Distinct().ToList();
        
        return query;
    }


    public bool AssignSuperiorsToUser(List<long> superiorsIds, UserBase user) {
        List<UserSuperiorXRefs> resultRange = new();
        foreach (var superiorId in superiorsIds) {
            var newRefs = new UserSuperiorXRefs{
                IdUser = user.Id,
                IdSuperior = superiorId,
                AssignedDate = DateTime.Now
            };
            resultRange.Add(newRefs);
        }
        _db.UserSuperiorXRefs.AddRange(resultRange);
        
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }

    public bool UnassignSuperiorsFromUser(List<long> superiorsIds, UserBase user){
        var result = _db.UserSuperiorXRefs.Where(
            o => o.IdUser == user.Id && superiorsIds.Contains(o.IdSuperior));
        if (!result.Any()) {
            return false;
        }
        _db.RemoveRange(result);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }

    public IEnumerable<UserBase> GetAllUsersUnderProject(long projectId) {
        var query = _db.UserProjectXRefs.Where(x => x.IdProject == projectId).Join(_db.User,
            refs => refs.IdUser,
            user => user.Id,
            (refs, user) => user);

        return query;
    }
    

    public bool AssignUsersToProject(List<long> usersIds, Project project) {
        List<UserProjectXRefs> resultRange = new();
        foreach (var userId in usersIds) {
            var newRefs = new UserProjectXRefs {
                IdUser = userId,
                IdProject = project.Id,
                AssignedDate = DateTime.Now,
                Id = default
            };
            resultRange.Add(newRefs);
        }
        _db.UserProjectXRefs.AddRange(resultRange);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }


    public bool UnassignUsersFromProject(List<long> usersIds, Project project) {
        var result = _db.UserProjectXRefs.Where(
            o => usersIds.Contains(o.IdUser) && o.IdProject == project.Id);
        if (!result.Any()) {
            return false;
        }
        
        _db.RemoveRange(result);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }


    public bool IsUserAssignedToProject(User user, Project project) {

        var reference = _db.UserProjectXRefs.SingleOrDefault(
            refs => refs.IdProject == project.Id && refs.IdUser == user.Id
            );

        return reference != null;
    }
    
    public void DeleteUser(int id) {
        var dummyUser = new User {
            Id = id
        };
        _db.User.Remove(dummyUser);
        _db.SaveChanges();
    }
}