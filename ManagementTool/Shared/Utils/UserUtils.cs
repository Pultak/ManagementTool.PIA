using System.Net.Mail;
using System.Text.RegularExpressions;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Shared.Utils;

public static class UserUtils {

    public const string PasswordRegexPattern = "^.*(?=.{8,})(?=.*[a-z])(?=.*[A-Z]).*$";
    public static Regex PasswordRegex = new(PasswordRegexPattern);
    
    public const string EmailRegexPattern = "^([a-zA-Z0-9_\\.\\-])+\\@(([a-zA-Z0-9\\-])+\\.)+([a-zA-Z0-9]{2,4})+$";
    public static Regex EmailRegex = new(EmailRegexPattern);

    

    //public static Regex FullNameRegex = new("\\w+, [\\w]+[ \\w+]*");
    public static Regex SpecialCharactersRegex = new("^[a-zA-Z0-9 ]*$");

    public const int MinUsernameLength = 4;
    public const int MaxUsernameLength = 32;

    public const int MinFullnameLength = 2;
    public const int MaxFullnameLength = 124;
    
    public const int MinPwdLength = 8;
    public const int MaxPwdLength = 32;
    
    public const int MinWorkplaceNameLength = 1;
    public const int MaxWorkplaceNameLength = 64;



    public static bool IsValidPassword(string password) {
        if (password.Length < MinPwdLength || password.Length > MaxPwdLength
            && !PasswordRegex.IsMatch(password)) {
            return false;
        }

        return true;
    }

    public static EUserCreationResponse ValidateUser(UserBase? user) {
        if (user == null) {
            return EUserCreationResponse.EmptyUser;
        }
        if (user.Username.Length < MinUsernameLength || user.Username.Length > MaxUsernameLength
            && SpecialCharactersRegex.IsMatch(user.Username)) {
            return EUserCreationResponse.InvalidUsername;
        }

        if (user.FullName.Length < MinFullnameLength || user.FullName.Length > MaxFullnameLength) {
            return EUserCreationResponse.InvalidFullName;
        }

        if (user.PrimaryWorkplace.Length < MinWorkplaceNameLength || user.PrimaryWorkplace.Length > MaxWorkplaceNameLength) {
            return EUserCreationResponse.InvalidWorkplace;
        }


        if (!EmailRegex.IsMatch(user.EmailAddress)) {
            return EUserCreationResponse.InvalidEmail;
        }
        
        return EUserCreationResponse.Ok;
    }


    public static string CreateRandomPassword(int passwordLength) {
        var allowedChars = "0123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ*/-+!@#$%^&(";
        var randNum = new Random();
        var chars = new char[passwordLength];
        var allowedCharCount = allowedChars.Length;
        for (var i = 0; i < passwordLength; i++) {
            chars[i] = allowedChars[(int)((allowedCharCount) * randNum.NextDouble())];
        }
        return new string(chars);
    }


    public static bool IsUserAuthorized(LoggedUserPayload? loggedUser, ERoleType? role) {

        if (loggedUser == null) {
            return false;
        }

        if (role == null ) {
            return true;
        }

        return loggedUser.Roles != null 
               && loggedUser.Roles.Any(userRole => userRole.Type == role);
    }
    
}

