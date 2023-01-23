using ManagementTool.Client.Utils;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;
using Microsoft.AspNetCore.Components;

namespace ManagementTool.Client.Pages; 

public abstract class InteractivePageBase<T>: ComponentBase, IDisposable {
    public const string SubmitBtnId = "submitButton";

    [Inject]
    protected StateContainer<LoggedUserPayload> LoggedUserContainer { get; set; }
    
    [Inject]
    protected ILogger<T> Logger { get; set; }

    [Inject]
    protected NavigationManager UriHelper { get; set; }


    protected ERoleType[]? NeededRoles = null;
    protected bool IsAuthorized => UserUtils.IsUserAuthorized(LoggedUserContainer.Value, NeededRoles);

    /// <summary>
    ///     Conditional message that will be showed in exceptional situations (API failure etc)
    /// </summary>
    protected string? ExceptionMessage { get; set; } = null;

    /// <summary>
    ///     Flag indicating loading of data from the API
    /// </summary>
    protected bool WaitingForApiResponse { get; set; } = true;

    protected string ReturnUri { get; set; } = "/";


    protected override void OnInitialized() {
        //register event handler for the change of the logged user
        LoggedUserContainer.OnStateChange += StateHasChanged;
        base.OnInitialized();
    }



    protected override async Task OnInitializedAsync() {
        //register event handler for the change of the logged user
        StateHasChanged();
        LoggedUserContainer.OnStateChange += StateHasChanged;
        WaitingForApiResponse = false;
        await base.OnInitializedAsync();
    }


    protected void Return() {
        UriHelper.NavigateTo(ReturnUri);
    }

    

    /// <summary>
    /// Method from IDisposable that will be called once this component is disposed off
    /// Preventing possible memory leaks
    /// </summary>
    public virtual void Dispose() {
        LoggedUserContainer.OnStateChange -= StateHasChanged;
    }

    
    protected virtual void ResolveResponse(EApiHttpResponse apiResponse, bool changePage) {
        WaitingForApiResponse = false;
        if (apiResponse == EApiHttpResponse.Ok) {
            Return();
        }
        else {
            ExceptionMessage = StringResolver.ResolveApiResponse(apiResponse);
            StateHasChanged();
        }
    }

}