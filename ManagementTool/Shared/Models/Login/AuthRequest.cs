using System.ComponentModel.DataAnnotations;
using ManagementTool.Shared.Utils;

namespace ManagementTool.Shared.Models.Login;

/// <summary>
///     Login page model with all needed validation quotations
/// </summary>
public class AuthRequest {
    public AuthRequest() {
    }

    public AuthRequest(string? username, string? password) {
        Username = username;
        Password = password;
    }

    /// <summary>
    ///     Username from the login form
    /// </summary>
    [Required(ErrorMessage = "Orion jméno musí být vyplněno!")]
    [StringLength(UserUtils.MaxUsernameLength, MinimumLength = UserUtils.MinUsernameLength,
        ErrorMessage = "Orion jméno musí být mezi 3 a 32 znaky!")]
    [RegularExpression(UserUtils.SpecialCharactersRegexPattern,
        ErrorMessage = "Speciální znaky do uživatelského jména nepatří!")]
    public string? Username { get; set; }

    /// <summary>
    ///     Password passed from the login form
    /// </summary>
    [Required(ErrorMessage = "Heslo musí být vyplněno!")]
    [StringLength(100, MinimumLength = 1,
        ErrorMessage = "Heslo nemůže být tak dlouhé!")]
    public string? Password { get; set; }
}