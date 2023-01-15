﻿using ManagementTool.Shared.Models.Utils;
using Syncfusion.Blazor.Schedule;

namespace ManagementTool.Client.Utils; 

public class StringResolver {

    public static string ResolveApiResponse(EApiHttpResponse response) {
        return response switch {
            EApiHttpResponse.Ok => "Vše proběhlo v pořádku",
            EApiHttpResponse.HttpRequestException => "Nepovedlo se připojit k serveru. Zkuste to znovu později!",
            EApiHttpResponse.ArgumentException => "Server zaslal invalidní data!",
            EApiHttpResponse.UnknownException => "Něco se nepovedlo při posílání dat na server!",
            EApiHttpResponse.InvalidData => "Chyba vstupu zasílaného na API!",
            EApiHttpResponse.ConflictFound => "Objekt pod stejným jménem již existuje",
            EApiHttpResponse.HttpResponseException => "Byla přijata chyba HttpResponseException od API, ale s neznámých HTTP kódem.",
            _ => throw new ArgumentOutOfRangeException(nameof(response), response, null)
        };
    }
    public static string ResolveUserValidation(EUserCreationResponse response) {
        return response switch {
            EUserCreationResponse.Ok => "Vše proběhlo v pořádku",
            EUserCreationResponse.EmptyUser => "Uživateleské data nesmí být prázdné!",
            EUserCreationResponse.InvalidUsername => "Nevalidní orion jméno poskytnuto!",
            EUserCreationResponse.InvalidPassword => "Nedostatečně silné heslo poskytnuto!",
            EUserCreationResponse.InvalidFullName => "Celé jméno uživatele je nevalidní",
            EUserCreationResponse.InvalidEmail => "Email v nevalidním formátu poskutnut!",
            EUserCreationResponse.InvalidWorkplace => "Nevalidní pracovní místo vloženo!",
            EUserCreationResponse.UsernameTaken => "Uživatelské jméno již v systému existuje!",
            _ => throw new ArgumentOutOfRangeException(nameof(response), response, null)
        };
    }
    public static string ResolveProjectValidation(EProjectCreationResponse response) {
        return response switch {
            EProjectCreationResponse.EmptyProject => "Data projektu nesmí být prázdná!",
            EProjectCreationResponse.InvalidName => "Nevalidní jméno vloženo!",
            EProjectCreationResponse.InvalidFromDate => "Nevalidní \"od\" datum!",
            EProjectCreationResponse.InvalidToDate => "Nevalidní \"do\" datum!",
            EProjectCreationResponse.InvalidDescription => "Nevalidní popisek!",
            EProjectCreationResponse.NameTaken => "Jméno projektu již existuje!",
            EProjectCreationResponse.Ok => "Vše proběhlo v pořádku",
            _ => throw new ArgumentOutOfRangeException(nameof(response), response, null)
        };
    }


    public static string ResolveAssignmentState(EAssignmentState state) {
        return state switch {
            EAssignmentState.Active => "Aktivní",
            EAssignmentState.Draft => "Rozdělaný (neaktivní)",
            EAssignmentState.Cancelled => "Zrušený (neaktivní)",
            EAssignmentState.Done => "Hotový (neaktivní)",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }

    public static string ResolveAssignmentValidation(EAssignmentCreationResponse validation) {
        return validation switch {
            EAssignmentCreationResponse.Empty => "Data úkolu nesmí být prazdná!",
            EAssignmentCreationResponse.InvalidProject => "Nevalidní projekt zvolen!",
            EAssignmentCreationResponse.InvalidUser => "Nevalidní uživatel zvolen!",
            EAssignmentCreationResponse.UserNotAssignedToProject => "Zvolený uživatel není součástí projektu!",
            EAssignmentCreationResponse.InvalidName => "Bylo zvoleno nevalidní jméno úkolu!",
            EAssignmentCreationResponse.InvalidNote => "Byla zvolen nevalidní poznámka k úkolu!",
            EAssignmentCreationResponse.InvalidAllocationScope => "Přiřazený časový rozsah není validní!",
            EAssignmentCreationResponse.InvalidFromDate => "Zvolený datum od není možné použít!",
            EAssignmentCreationResponse.InvalidToDate => "Zvolený datum do není možné použít!",
            EAssignmentCreationResponse.Ok => "Vše proběhlo v pořádku!",
            _ => throw new ArgumentOutOfRangeException(nameof(validation), validation, null)
        };
    }


    public static string ResolveTimeScope(long scope) {
        //40 hours in week
        var fte = scope / 40;
        var fteString = fte > 0 ? $"{fte}fte " : "";

        var partScope = scope - fte * 40;

        var days = partScope / 24;
        var dayString = days > 0 ? $"{days} d " : "";

        var hours = partScope - days * 24;
        var hoursString = hours > 0 ? $"{hours} h" : "";

        return $"{fteString}{dayString}{hoursString}";
    }
}