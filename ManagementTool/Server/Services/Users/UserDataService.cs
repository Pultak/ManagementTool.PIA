using ManagementTool.Shared.Models.Database;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
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
        var rowsChanged = _db.SaveChanges();
        return rowsChanged > 0;
    }

    public IEnumerable<User> GetAllUsersByRole(Role role) {
        var queryResult = _db.UserRoleXRefs.Join(_db.User,
                refs => refs.IdRole,
                user => user.Id,
                (refs, user) => new {
                    id = user.Id,
                    username = user.Username,
                    pwd = user.Pwd,
                    fullName = user.FullName,
                    primaryWorkplace = user.PrimaryWorkplace,
                    emailAddress = user.EmailAddress,
                    roleId = refs.IdRole
                }).Where(x => x.roleId == role.Id).
            Select(x => new User(x.id, x.username, x.pwd, x.fullName, x.primaryWorkplace, x.emailAddress)).ToList();
        return queryResult;
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
        /*todo remove
         var emp = _db.User.Find(id);
        if (emp == null) {
            return;
        }*/
        var dummyUser = new User {
            Id = id
        };
        _db.User.Remove(dummyUser);
        _db.SaveChanges();
    }
}