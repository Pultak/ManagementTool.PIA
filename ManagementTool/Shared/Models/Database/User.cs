using System.ComponentModel.DataAnnotations;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Shared.Models.Database;

public class User
{

    public long Id { get; set; }
    
    [Required(ErrorMessage = "Orion jméno musí být vyplněno!")]
    [StringLength(UserUtils.MaxUsernameLength, MinimumLength = UserUtils.MinUsernameLength, 
        ErrorMessage = "Orion jméno musí být mezi 3 a 80 znaky!")]
    public string Username { get; set; }

    public string Pwd { get; set; }


    [Required(ErrorMessage = "Jméno uživatele musí být vyplněno!")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Pracovní místo musí být vyplněno!")]
    [StringLength(UserUtils.MaxWorkplaceNameLength, MinimumLength = UserUtils.MinWorkplaceNameLength,
        ErrorMessage = "Název pracovního místa musí být mezi 1 a 64 znaky!")]
    public string PrimaryWorkplace { get; set; }

    [Required(ErrorMessage = "Emailová adresa musí být vyplněna!")]
    [DataType(DataType.EmailAddress, ErrorMessage = "Nejedná se o validní emailovou adresu!")]
    [EmailAddress]
    public string EmailAddress { get; set; }

    public bool PwdInit { get; set; }

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