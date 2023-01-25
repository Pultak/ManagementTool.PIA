namespace ManagementTool.Client.Utils;


/// <summary>
/// Container to hold the applications needed data models
/// </summary>
/// <typeparam name="T">Type of the data to be contained</typeparam>
public class StateContainer<T> {
    public T? Value { get; set; }
    public event Action OnStateChange = () => {};

    public void SetValue(T? value) {
        Value = value;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChange?.Invoke();
}