using ManagementTool.Shared.Models.ApiModels;
using ManagementTool.Shared.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ManagementTool.Server.Services.Users;

public class UserDataService : IUserDataService{
    private readonly ManToolDbContext _db; 
    public UserDataService(ManToolDbContext db){
        _db = db;
    }

    public IEnumerable<User> GetAllUsers() {
        return _db.User.ToList();
    }



    public long AddUser(User user) {
        _db.User.Add(user);
        if (_db.SaveChanges() <= 0) {
            return -1;
        }
        return user.Id;
    }

    public bool DeleteUser(long id) {
        var dbUser = GetUserById(id);
        if (dbUser == null) {
            return false;
        }
        //_db.User.Attach(dummyUser);
        _db.User.Remove(dbUser);
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
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

    public IEnumerable<User> GetAllUsersByRole(Role role) {
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
                x.emailAddress, x.pwd, x.salt, x.pwdInit)).ToList();
        return queryResult;
    }

     public IEnumerable<long> GetAllUserSuperiorsIds(long userId) {
         var queryResult = _db.UserSuperiorXRefs.Where(x => x.IdUser == userId).Select(x => x.IdSuperior);
        return queryResult;
    }



    public IEnumerable<DataModelAssignment<UserBase>> GetAllUsersAssignedToProject(long projectId) {

        var query = from user in _db.User
            from refs in _db.UserProjectXRefs.Where(x => x.IdProject == projectId).DefaultIfEmpty()
            select new DataModelAssignment<UserBase>(refs != null, new UserBase(user.Id, user.Username, user.FullName,
                user.PrimaryWorkplace, user.EmailAddress, user.PwdInit));
        /*join refs in _db.UserProjectXRefs 
            on user.Id equals refs.IdUser into allCols
        from subRefs in allCols.DefaultIfEmpty()
        group subRefs by user into grouped
        select new{grouped.Key.};*/

        /*var result = _db.User.GroupJoin(_db.UserProjectXRefs,
        user => user.Id,
        refs => refs.IdUser,
        (user, refs) => new {
            id = user.Id,
            username = user.Username,
            fullName = user.FullName,
            workplace = user.PrimaryWorkplace,
            email = user.EmailAddress,
            pwdInit = user.PwdInit,
            superiorId = user.SuperiorId,
            projectId = refs == null ? 0 : refs.IdProject
        }).Where(x => x.projectId == projectId || x.projectId == 0)
    .Select(x => new DataModelAssignment<UserBase>(
        x.projectId != 0, 
        new UserBase(x.id, x.username, x.fullName, x.workplace, x.email, x.superiorId))
    ).ToList();

*/
        //todo empty return
        return query;
    }

    public IEnumerable<UserBase> GetAllUsersUnderProject(long projectId) {
        var query = _db.UserProjectXRefs.Where(x => x.IdProject == projectId).Join(_db.User,
            refs => refs.IdUser,
            user => user.Id,
            (refs, user) => user);

        return query;
    }


    public bool AssignUserToProject(User user, Project project) {
        var newRefs = new UserProjectXRefs {
            IdUser = user.Id,
            IdProject = project.Id,
            AssignedDate = DateTime.Now
        };
        
        _db.UserProjectXRefs.Add(newRefs);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }


    public bool UnassignUserFromProject(User user, Project project) {
        var result = _db.UserProjectXRefs.Where(
            o => o.IdUser == user.Id && o.IdProject == project.Id);
        if (!result.Any()) {
            return false;
        }
        _db.Remove(result);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }


    public bool IsUserAssignedToProject(User user, Project project) {

        var reference = _db.UserProjectXRefs.SingleOrDefault(
            refs => refs.IdProject == project.Id && refs.IdUser == user.Id
            );

        return reference != null;
    }
    
    public User? GetUserById(int id) {
        var user = _db.User.Find(id);
        return user;
    }
    
    public void DeleteUser(int id) {
        var dummyUser = new User {
            Id = id
        };
        _db.User.Remove(dummyUser);
        _db.SaveChanges();
    }
}