using System.ComponentModel.DataAnnotations;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Shared.Models.Database; 

public class UserBase {
    public long Id { get; set; }

    [Required(ErrorMessage = "Orion jméno musí být vyplněno!")]
    [StringLength(UserUtils.MaxUsernameLength, MinimumLength = UserUtils.MinUsernameLength,
        ErrorMessage = "Orion jméno musí být mezi 3 a 32 znaky!")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Jméno uživatele musí být vyplněno!")]
    [StringLength(UserUtils.MaxFullnameLength, MinimumLength = UserUtils.MinFullnameLength,
        ErrorMessage = "Celé jméno uživatele musí být mezi 2 a 124 znaky!")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Pracovní místo musí být vyplněno!")]
    [StringLength(UserUtils.MaxWorkplaceNameLength, MinimumLength = UserUtils.MinWorkplaceNameLength,
        ErrorMessage = "Název pracovního místa musí být mezi 1 a 64 znaky!")]
    public string PrimaryWorkplace { get; set; }

    [Required(ErrorMessage = "Emailová adresa musí být vyplněna!")]
    [RegularExpression(UserUtils.EmailRegexPattern, ErrorMessage = "Nejedná se o validní emailovou adresu!")]
    public string EmailAddress { get; set; }

    public bool PwdInit { get; set; }



    public UserBase() {
        Username = "Unknown";
        FullName = "Unknown";
        PrimaryWorkplace = "Unknown";
        EmailAddress = "Unknown";
    }

    public UserBase(long id, string username,  string fullName, string primaryWorkplace,
        string emailAddress) {
        Id = id;
        Username = username;
        FullName = fullName;
        PrimaryWorkplace = primaryWorkplace;
        EmailAddress = emailAddress;
    }

}