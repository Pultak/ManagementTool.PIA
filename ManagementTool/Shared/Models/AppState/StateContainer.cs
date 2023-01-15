using ManagementTool.Shared.Models.Login;

namespace ManagementTool.Shared.Models.AppState;

public class StateContainer<T> {
    public T? Value { get; set; }
    public event Action OnStateChange;

    public void SetValue(T? value) {
        Value = value;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChange?.Invoke();
}