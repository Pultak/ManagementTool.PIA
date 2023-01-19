namespace ManagementTool.Shared.Models.Database;

public class User : UserBase {
    
    public string Pwd { get; set; }
    
    public string Salt { get; set; }


    public User() {

    }
    
    public User(UserBase userBase): base(userBase.Id, userBase.Username, userBase.FullName, 
        userBase.PrimaryWorkplace, userBase.EmailAddress, userBase.PwdInit) {

    }
    
    public User(long id, string username,  string fullName, string primaryWorkplace, string emailAddress,
        string pwd, string salt, bool pwdInit) : base(id, username, fullName, primaryWorkplace, emailAddress, pwdInit) {
        Pwd = pwd;
        Salt = salt;
    }
}