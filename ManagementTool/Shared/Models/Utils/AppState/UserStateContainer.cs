using ManagementTool.Shared.Models.Login;

namespace ManagementTool.Shared.Models.Utils.AppState;

public class UserStateContainer {
    public LoggedUserPayload? Value { get; set; }
    public event Action OnStateChange;

    public void SetValue(LoggedUserPayload? value) {
        Value = value;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChange?.Invoke();
}