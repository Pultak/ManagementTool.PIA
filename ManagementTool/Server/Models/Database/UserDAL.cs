namespace ManagementTool.Server.Models.Database;

public class UserDAL {

    public long Id { get; set; }
    
    public string Username { get; set; }
    
    public string FullName { get; set; }
    
    public string PrimaryWorkplace { get; set; }
    
    public string EmailAddress { get; set; }

    public bool PwdInit { get; set; }


    public string Pwd { get; set; }
    
    public string Salt { get; set; }


    public UserDAL() {
        Username = string.Empty;
        FullName = string.Empty;
        PrimaryWorkplace = string.Empty;
        EmailAddress = string.Empty;
        Pwd = string.Empty;
        Salt = string.Empty;
    }
    
    /*public UserDAL(UserBase userBase, string pwd, string salt): base(userBase.Id, userBase.Username, userBase.FullName, 
        userBase.PrimaryWorkplace, userBase.EmailAddress, userBase.PwdInit) {
        Pwd = pwd;
        Salt = salt;
    }*/
    
    public UserDAL(long id, string username,  string fullName, string primaryWorkplace, string emailAddress,
        string pwd, string salt, bool pwdInit){
        Username = username;
        FullName = fullName;
        PrimaryWorkplace = primaryWorkplace;
        EmailAddress = emailAddress;
        Pwd = pwd;
        Salt = salt;
    }
}