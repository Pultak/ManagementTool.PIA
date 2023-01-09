using System.Net.Mail;
using System.Text.RegularExpressions;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Shared.Utils;

public static class UserUtils {
    public static Regex PasswordRegex = new("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!*@#$%^&+=]).*$");
    public static Regex FullNameRegex = new("\\w+, [\\w]+[ \\w+]*");
    public static Regex SpecialCharactersRegex = new("^[a-zA-Z0-9 ]*$");

    public const int MinUsernameLength = 4;
    public const int MaxUsernameLength = 32;
    
    public const int MinPwdLength = 4;
    public const int MaxPwdLength = 32;
    
    public const int MinWorkplaceNameLength = 1;
    public const int MaxWorkplaceNameLength = 64;



    public static bool ValidatePassword(string password) {

        if (password.Length < MinPwdLength || password.Length < MaxPwdLength
            && !PasswordRegex.IsMatch(password)) {
            return false;
        }

        return true;
    }

    public static EUserCreationResponse ValidateUser(User? user) {
        if (user == null) {
            return EUserCreationResponse.EmptyUser;
        }
        if (user.Username.Length < MinUsernameLength || user.Username.Length > MaxUsernameLength
            && SpecialCharactersRegex.IsMatch(user.Username)) {
            return EUserCreationResponse.InvalidUsername;
        }

        if (!FullNameRegex.IsMatch(user.FullName)) {
            //todo czech characters might be a problem for regex
            return EUserCreationResponse.InvalidFullName;
        }

        if (user.PrimaryWorkplace.Length < MinWorkplaceNameLength || user.PrimaryWorkplace.Length > MaxWorkplaceNameLength) {
            return EUserCreationResponse.InvalidWorkplace;
        }

        try {
            //validation of email address
            MailAddress parsedAddress = new(user.EmailAddress);
        }
        catch (FormatException e) {
            return EUserCreationResponse.InvalidEmail;
        }

        return EUserCreationResponse.Ok;
    }
    
}

