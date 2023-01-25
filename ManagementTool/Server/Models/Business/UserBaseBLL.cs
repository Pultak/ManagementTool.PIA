namespace ManagementTool.Server.Models.Business;

/// <summary>
/// Business logic layer user model containing all information our user need
/// </summary>
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

    /// <summary>
    /// Username (orion) of the user
    /// it is used for logging in the system and other crucial things
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Full name of the user
    /// There is currently no specified format
    /// Should contain both first and last name
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Name of the workplace this user works in
    /// </summary>
    public string PrimaryWorkplace { get; set; }

    /// <summary>
    /// Assigned email address
    /// Can be used to sent emails to the user
    /// </summary>
    public string EmailAddress { get; set; }

    /// <summary>
    /// Flag indicating that the user still has auto generated password
    /// that should be changed
    /// </summary>
    public bool PwdInit { get; set; }
}
