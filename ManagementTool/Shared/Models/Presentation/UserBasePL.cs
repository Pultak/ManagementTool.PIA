using System.ComponentModel.DataAnnotations;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Shared.Models.Presentation;

/// <summary>
/// Presentation layer user model  containing all information our
/// user need with data validation annotations 
/// </summary>
public class UserBasePL {
    public UserBasePL() {
        Username = string.Empty;
        FullName = string.Empty;
        PrimaryWorkplace = string.Empty;
        EmailAddress = string.Empty;
    }

    public UserBasePL(long id, string username, string fullName, string primaryWorkplace,
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
    [Required(ErrorMessage = "Orion jméno musí být vyplněno!")]
    [StringLength(UserUtils.MaxUsernameLength, MinimumLength = UserUtils.MinUsernameLength,
        ErrorMessage = "Orion jméno musí být mezi 3 a 32 znaky!")]
    [RegularExpression(UserUtils.SpecialCharactersRegexPattern,
        ErrorMessage = "Speciální znaky do uživatelského jména nepatří!")]
    public string Username { get; set; }

    /// <summary>
    /// Full name of the user
    /// There is currently no specified format
    /// Should contain both first and last name
    /// </summary>
    [Required(ErrorMessage = "Jméno uživatele musí být vyplněno!")]
    [StringLength(UserUtils.MaxFullnameLength, MinimumLength = UserUtils.MinFullnameLength,
        ErrorMessage = "Celé jméno uživatele musí být mezi 4 a 124 znaky!")]
    public string FullName { get; set; }

    /// <summary>
    /// Name of the workplace this user works in
    /// </summary>
    [Required(ErrorMessage = "Pracovní místo musí být vyplněno!")]
    [StringLength(UserUtils.MaxWorkplaceNameLength, MinimumLength = UserUtils.MinWorkplaceNameLength,
        ErrorMessage = "Název pracovního místa musí být mezi 1 a 64 znaky!")]
    public string PrimaryWorkplace { get; set; }

    /// <summary>
    /// Assigned email address
    /// Can be used to sent emails to the user
    /// </summary>
    [Required(ErrorMessage = "Emailová adresa musí být vyplněna!")]
    [RegularExpression(UserUtils.EmailRegexPattern, ErrorMessage = "Nejedná se o validní emailovou adresu!")]
    public string EmailAddress { get; set; }

    /// <summary>
    /// Flag indicating that the user still has auto generated password
    /// that should be changed
    /// </summary>
    public bool PwdInit { get; set; }
}
