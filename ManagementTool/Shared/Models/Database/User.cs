namespace ManagementTool.Shared.Models.Database;

public class User
{

    public long Id { get; set; }

    public string Username { get; set; }

    public string Pwd { get; set; }

    public string FullName { get; set; }

    public string PrimaryWorkplace { get; set; }

    public string EmailAddress { get; set; }




    public User(long id, string username, string pwd, string fullName, string primaryWorkplace, string emailAddress) {
        Id = id;
        Username = username;
        Pwd = pwd;
        FullName = fullName;
        PrimaryWorkplace = primaryWorkplace;
        EmailAddress = emailAddress;
    }

    public User(long id) {
        Id = id;
    }

}