namespace ManagementTool.Server.Models.Business;

public class UserBaseBLL {
    public UserBaseBLL() {
        Username = string.Empty;
        FullName = string.Empty;
        PrimaryWorkplace = string.Empty;
        EmailAddress = string.Empty;
    }

    public UserBaseBLL(long id, string username, string fullName, string primaryWorkplace,
        string emailAddress, bool pwdInit) {
        Id = id;
        Username = username;
        FullName = fullName;
        PrimaryWorkplace = primaryWorkplace;
        EmailAddress = emailAddress;
        PwdInit = pwdInit;
    }

    public long Id { get; set; }

    public string Username { get; set; }

    public string FullName { get; set; }

    public string PrimaryWorkplace { get; set; }

    public string EmailAddress { get; set; }

    public bool PwdInit { get; set; }
}
