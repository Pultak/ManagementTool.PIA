using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Login;

namespace ManagementTool.Shared.Models.AppState;

public class EditUserStateContainer
{

    public UserBase? Value { get; set; }
    public event Action OnStateChange;

    public void SetValue(UserBase? value)
    {
        Value = value;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChange?.Invoke();
}