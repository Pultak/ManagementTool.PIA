using ManagementTool.Shared.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ManagementTool.Server.Services.Users;

public class UserDataService : IUserDataService{
    private readonly ManToolDbContext _db; //To Get all employees details


    //TODO implement these bois

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

        //todo check if true id
        return user.Id;
    }

    public void DeleteUser(long id) {
        var dummyUser = new User(id);
        _db.User.Remove(dummyUser);
        _db.SaveChanges();
    }

    public User? GetUserById(long id) {
        return _db.User.Find(id);
    }

    public User? GetUserByName(string username) {
        return _db.User.SingleOrDefault(user => string.Equals(user.Username, username));
    }
    

    //To Update the records of a particluar employee
    public void UpdateUser(User user) {
        _db.Entry(user).State = EntityState.Modified;
        _db.SaveChanges();
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


        throw new NotImplementedException();



    }

    public bool IsUserAssignedToProject(User user, Project project) {
        throw new NotImplementedException();
    }

    //Get the details of a particular employee
    public User? GetUserById(int id) {
        var user = _db.User.Find(id);
        return user;
    }

    //To Delete the record of a particular employee
    public void DeleteUser(int id) {
        var emp = _db.User.Find(id);
        if (emp == null) {
            return;
        }

        _db.User.Remove(emp);
        _db.SaveChanges();
    }
}