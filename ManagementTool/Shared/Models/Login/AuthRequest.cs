using ManagementTool.Shared.Utils;
using System.ComponentModel.DataAnnotations;

namespace ManagementTool.Shared.Models.Login; 

public class AuthRequest {

    [Required(ErrorMessage = "Orion jméno musí být vyplněno!")]
    [StringLength(UserUtils.MaxUsernameLength, MinimumLength = UserUtils.MinUsernameLength,
        ErrorMessage = "Orion jméno musí být mezi 3 a 32 znaky!")]
    [RegularExpression(UserUtils.SpecialCharactersRegexPattern,
        ErrorMessage = "Speciální znaky do uživatelského jména nepatří!")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Heslo musí být vyplněno!")]
    [StringLength(100, MinimumLength = 1,
        ErrorMessage = "Heslo nemůže být tak dlouhé!")]
    public string? Password { get; set; }

    public AuthRequest() {

    }

    public AuthRequest(string? username, string? password) {
        Username = username;
        Password = password;
    }
}