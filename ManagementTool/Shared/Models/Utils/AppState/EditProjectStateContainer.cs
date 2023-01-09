using ManagementTool.Shared.Models.Database;

namespace ManagementTool.Shared.Models.Utils.AppState; 

public class EditProjectStateContainer {
    public Project? Value { get; set; }
    public event Action OnStateChange;

    public void SetValue(Project? value) {
        Value = value;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChange?.Invoke();
}