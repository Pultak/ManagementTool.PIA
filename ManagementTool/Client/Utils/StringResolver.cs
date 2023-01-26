using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Client.Utils;

/// <summary>
/// Provides static methods to get the response string associated with the state of the given response object
/// </summary>
public class StringResolver {


    /// <summary>
    /// Resolve the API response status
    /// </summary>
    /// <param name="response">The API response status</param>
    /// <returns>String representation of the response</returns>
    public static string ResolveApiResponse(ApiHttpResponse response) {
        return response switch {
            ApiHttpResponse.Ok => "Vše proběhlo v pořádku",
            ApiHttpResponse.HttpRequestException => "Nepovedlo se připojit k serveru. Zkuste to znovu později!",
            ApiHttpResponse.ArgumentException => "Server zaslal invalidní data!",
            ApiHttpResponse.UnknownException => "Něco se nepovedlo při posílání dat na server!",
            ApiHttpResponse.InvalidData => "Chyba vstupu zasílaného na API!",
            ApiHttpResponse.ConflictFound => "Objekt pod stejným jménem již existuje",
            ApiHttpResponse.HttpResponseException =>
                "Byla přijata chyba HttpResponseException od API, ale s neznámých HTTP kódem.",
            ApiHttpResponse.Unauthorized =>
                "Vypadá to, že nemáte dostatečná práva pro používání následující komponenty!",
            _ => throw new ArgumentOutOfRangeException(nameof(response), response, null)
        };
    }

    /// <summary>
    /// Resolve the user creation status
    /// </summary>
    /// <param name="response">The user creation status</param>
    /// <returns>String representation of the response</returns>
    public static string ResolveUserValidation(UserCreationResponse response) {
        return response switch {
            UserCreationResponse.Ok => "Vše proběhlo v pořádku",
            UserCreationResponse.EmptyUser => "Uživatelské data nesmí být prázdné!",
            UserCreationResponse.InvalidUsername => "Nevalidní orion jméno poskytnuto!",
            UserCreationResponse.InvalidPassword => "Nedostatečně silné heslo poskytnuto!",
            UserCreationResponse.InvalidFullName => "Celé jméno uživatele je nevalidní",
            UserCreationResponse.InvalidEmail => "Email v nevalidním formátu poskytnout!",
            UserCreationResponse.InvalidWorkplace => "Nevalidní pracovní místo vloženo!",
            UserCreationResponse.UsernameTaken => "Uživatelské jméno již v systému existuje!",
            _ => throw new ArgumentOutOfRangeException(nameof(response), response, null)
        };
    }

    /// <summary>
    /// Resolve the project creation status
    /// </summary>
    /// <param name="response">The project creation status</param>
    /// <returns>String representation of the response</returns>
    public static string ResolveProjectValidation(ProjectCreationResponse response) {
        return response switch {
            ProjectCreationResponse.EmptyProject => "Data projektu nesmí být prázdná!",
            ProjectCreationResponse.InvalidName => "Nevalidní jméno vloženo!",
            ProjectCreationResponse.InvalidFromDate => "Nevalidní \"od\" datum!",
            ProjectCreationResponse.InvalidToDate => "Nevalidní \"do\" datum!",
            ProjectCreationResponse.InvalidDescription => "Nevalidní popisek!",
            ProjectCreationResponse.NameTaken => "Jméno projektu již existuje!",
            ProjectCreationResponse.Ok => "Vše proběhlo v pořádku",
            ProjectCreationResponse.InvalidRoleName => "Nepovedlo se přiřadit novou roli pro tento projekt!",
            _ => throw new ArgumentOutOfRangeException(nameof(response), response, null)
        };
    }


    /// <summary>
    /// Resolve the assignment current state
    /// </summary>
    /// <param name="state">The state of the assignment</param>
    /// <returns>String representation of the state</returns>
    public static string ResolveAssignmentState(AssignmentState state) {
        return state switch {
            AssignmentState.Active => "Aktivní",
            AssignmentState.Draft => "Rozdělaný (neaktivní)",
            AssignmentState.Cancelled => "Zrušený (neaktivní)",
            AssignmentState.Done => "Hotový (neaktivní)",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }


    /// <summary>
    /// Resolve the assignment creation status
    /// </summary>
    /// <param name="validation">The assignment creation status</param>
    /// <returns>String representation of the status</returns>
    public static string ResolveAssignmentValidation(AssignmentCreationResponse validation) {
        return validation switch {
            AssignmentCreationResponse.Empty => "Data úkolu nesmí být prázdná!",
            AssignmentCreationResponse.InvalidProject => "Nevalidní projekt zvolen!",
            AssignmentCreationResponse.InvalidUser => "Nevalidní uživatel zvolen!",
            AssignmentCreationResponse.UserNotAssignedToProject => "Zvolený uživatel není součástí projektu!",
            AssignmentCreationResponse.InvalidName => "Bylo zvoleno nevalidní jméno úkolu!",
            AssignmentCreationResponse.InvalidNote => "Byla zvolen nevalidní poznámka k úkolu!",
            AssignmentCreationResponse.InvalidAllocationScope => "Přiřazený časový rozsah není validní!",
            AssignmentCreationResponse.InvalidFromDate => "Zvolený datum od není možné použít (nesmí být dřív než počátek projektu)!",
            AssignmentCreationResponse.InvalidToDate => "Zvolený datum do není možné použít!",
            AssignmentCreationResponse.Ok => "Vše proběhlo v pořádku!",
            _ => throw new ArgumentOutOfRangeException(nameof(validation), validation, null)
        };
    }


    /// <summary>
    /// Resolve the authentication outcome
    /// </summary>
    /// <param name="response">The outcome</param>
    /// <returns>String representation of the response</returns>
    public static string ResolveAuthenticationResponse(AuthResponse? response) {
        switch (response) {
            case AuthResponse.EmptyUsername:
            case AuthResponse.EmptyPassword:
                return "Před stisknutím tlačítka pro přihlášení, prosím vyplňte potřebné údaje!";
            case AuthResponse.UnknownUser:
                return "Zadaný uživatel neexistuje!";
            case AuthResponse.WrongPassword:
                return "Bylo zadáno špatné heslo!";
            case AuthResponse.Success:
                return "Přihlášení proběhlo v pořádku!";
            case AuthResponse.AlreadyLoggedIn:
                return "Už jste v systému přihlášený!";
            case AuthResponse.UnknownResponse:
            case null:
                return "Něco se nepodařilo! Zkuste přihlášení znovu!";
            case AuthResponse.BadRequest:
                return "Data jenž byli zaslány na server byly nevalidní. Zkuste přihlášení znovu!";
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    /// <summary>
    ///  Resolves the allocation scope to more human readable format 
    /// </summary>
    /// <param name="scope"> scope you want to modify </param>
    /// <param name="fte"> flag indicating if fte should be used </param>
    /// <returns> string representation of the time scope  </returns>
    public static string ResolveTimeScope(long scope, bool fte) {
        var finalScope = fte ? scope * 40 : scope;
        return ResolveTimeScope(finalScope);
    }

    /// <summary>
    ///  Resolves the allocation scope to more human readable format 
    /// </summary>
    /// <param name="scope"> scope you want to modify </param>
    /// <returns> string representation of the time scope </returns>
    public static string ResolveTimeScope(long scope) {
        //40 work hours in a week
        var fte = scope / 40;
        var fteString = fte > 0 ? $"{fte} t " : "";

        var partScope = scope - fte * 40;

        var days = partScope / 8;
        var dayString = days > 0 ? $"{days} d " : "";

        var hours = partScope - days * 8;
        var hoursString = hours > 0 ? $"{hours} h" : "";

        return $"{fteString}{dayString}{hoursString}";
    }

    /// <summary>
    /// Resolve the workload validation status
    /// </summary>
    /// <param name="valid">The validation status</param>
    /// <returns>String representation of the status</returns>
    public static string ResolveWorkloadValidation(WorkloadValidation valid) {
        return valid switch {
            WorkloadValidation.Ok => "Validace vstupu proběhla v pořádku!",
            WorkloadValidation.EmptyUsers => "Pro pokračování musíte zvolit aspoň jednoho uživatele!",
            WorkloadValidation.InvalidFromDate => "Pro pokračování musíte zvolit validní od datum!",
            WorkloadValidation.InvalidToDate => "Pro pokračování musíte zvolit validní do datum!",
            WorkloadValidation.TooLongScope => "Zvolený časový rozsah je moc dlouhý!",
            _ => throw new ArgumentOutOfRangeException(nameof(valid), valid, null)
        };
    }
}