namespace ManagementTool.Shared.Models.Database;

public class User
{

    public long Id { get; set; }

    public string Username { get; set; }

    public string Pwd { get; set; }

    public string FullName { get; set; }

    public string PrimaryWorkplace { get; set; }

    public string EmailAddress { get; set; }

    public User(){}

    public User(long id, string username, string password, string fullName, string primaryWorkplace,
        string emailAddress) {
        Id = id;
        Username = username;
        Pwd = password;
        FullName = fullName;
        PrimaryWorkplace = primaryWorkplace;
        EmailAddress = emailAddress;
    }
}