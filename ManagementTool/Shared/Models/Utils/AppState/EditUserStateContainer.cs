using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Login;

namespace ManagementTool.Shared.Models.Utils.AppState; 

public class EditUserStateContainer {
    
    public User? Value { get; set; }
    public event Action OnStateChange;

    public void SetValue(User? value) {
        Value = value;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChange?.Invoke();
}