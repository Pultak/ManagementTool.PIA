using System.Text.RegularExpressions;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Shared.Utils;

public static class UserUtils {
    /// <summary>
    ///     Regex pattern for passwords that need to contain at least 8 characters.
    ///     It should contain at least one number, one small and also one big character
    /// </summary>
    public const string PasswordRegexPattern = "^.*(?=.{8,})(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).*$";

    /// <summary>
    ///     Regex pattern for checking of string email structure.
    ///     The standard emails go like 'email@domain.cz'
    /// </summary>
    public const string EmailRegexPattern = "^([a-zA-Z0-9_\\.\\-])+\\@(([a-zA-Z0-9\\-])+\\.)+([a-zA-Z0-9]{2,4})+$";

    /// <summary>
    ///     Regex pattern for checking if there is any special character in string.
    ///     This can be used to validate strings that should not contain any of these.
    /// </summary>
    public const string SpecialCharactersRegexPattern = "^[a-zA-Z0-9]*$";

    /// <summary>
    ///     Validation constants for users orion name (username)
    /// </summary>
    public const int MinUsernameLength = 3;

    public const int MaxUsernameLength = 32;


    /// <summary>
    ///     Validation constants for users full name
    /// </summary>
    public const int MinFullnameLength = 4;

    public const int MaxFullnameLength = 124;

    /// <summary>
    ///     Validation constants for users password length
    /// </summary>
    public const int MinPwdLength = 8;

    public const int MaxPwdLength = 32;


    /// <summary>
    ///     Validation constants for users workplace
    /// </summary>
    public const int MinWorkplaceNameLength = 1;

    public const int MaxWorkplaceNameLength = 64;
    public static Regex PasswordRegex = new(PasswordRegexPattern);
    public static Regex EmailRegex = new(EmailRegexPattern);
    public static Regex SpecialCharactersRegex = new(SpecialCharactersRegexPattern);


    /// <summary>
    ///     Checks passed password in string and validates if it is secure enough.
    ///     Note that the password should not be in hashed state
    /// </summary>
    /// <param name="password">not hashed password</param>
    /// <returns>true if valid</returns>
    public static bool IsValidPassword(string password) {
        if (password.Length < MinPwdLength || password.Length > MaxPwdLength
                                           || !PasswordRegex.IsMatch(password)) {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Checks if the passed user contains all required variables of needed quality.
    /// </summary>
    /// <param name="user">user base to check</param>
    /// <returns>Ok if valid, otherwise EUserCreationResponse with error</returns>
    public static UserCreationResponse ValidateUser(UserBasePL? user) {
        if (user == null) {
            return UserCreationResponse.EmptyUser;
        }

        if (user.Username.Length < MinUsernameLength || user.Username.Length > MaxUsernameLength
                                                     || !SpecialCharactersRegex.IsMatch(user.Username)) {
            return UserCreationResponse.InvalidUsername;
        }

        if (user.FullName.Length < MinFullnameLength || user.FullName.Length > MaxFullnameLength) {
            return UserCreationResponse.InvalidFullName;
        }

        if (user.PrimaryWorkplace.Length < MinWorkplaceNameLength ||
            user.PrimaryWorkplace.Length > MaxWorkplaceNameLength) {
            return UserCreationResponse.InvalidWorkplace;
        }


        if (string.IsNullOrEmpty(user.EmailAddress) || !EmailRegex.IsMatch(user.EmailAddress)) {
            return UserCreationResponse.InvalidEmail;
        }

        return UserCreationResponse.Ok;
    }


    /// <summary>
    ///     Creates random password of desired length. It contains random number, characters and special characters.
    ///     Note that it is possible that it can generate password that is not containing all required characters
    /// </summary>
    /// <param name="stringLength">Desired length of the newly generated password</param>
    /// <returns>newly generated password</returns>
    public static string CreateRandomString(int stringLength) {
        var allowedChars = "0123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ*/-+!@#$%^&(";
        var randNum = new Random();
        var chars = new char[stringLength];
        var allowedCharCount = allowedChars.Length;
        for (var i = 0; i < stringLength; i++) {
            chars[i] = allowedChars[(int)(allowedCharCount * randNum.NextDouble())];
        }

        return new string(chars);
    }

    /// <summary>
    ///     Method check if the passed user is authorized with desired Role
    /// </summary>
    /// <param name="loggedUser">user of which you want to check his roles</param>
    /// <param name="possibleRoles">Role you want to check</param>
    /// <returns>true if user is authorized</returns>
    public static bool IsUserAuthorized(LoggedUserPayload? loggedUser, RoleType[]? possibleRoles) {
        if (loggedUser == null) {
            return false;
        }

        //if wanted role is null then everything is possible
        if (possibleRoles == null) {
            return true;
        }

        //are there any unwanted roles?
        if (possibleRoles.Any(r => r == RoleType.NoRole)) {
            return false;
        }

        //is the user logged with desired role?
        var isAuthorized = loggedUser.Roles != null && possibleRoles.Any(
            roleType => loggedUser.Roles.Any(
                userRole => userRole.Type == roleType
            )
        );
        return isAuthorized;
    }
}

