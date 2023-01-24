using Microsoft.AspNetCore.Http;

namespace ManagementTool.ServerTests.MoqModels;

public class MockHttpSession : ISession {
    private readonly Dictionary<string, object> sessionStorage = new();

    public object this[string name] {
        get => sessionStorage[name];
        set => sessionStorage[name] = value;
    }

    string ISession.Id => throw new NotImplementedException();

    bool ISession.IsAvailable => throw new NotImplementedException();

    IEnumerable<string> ISession.Keys => sessionStorage.Keys;

    void ISession.Clear() {
        sessionStorage.Clear();
    }

    Task ISession.CommitAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

    Task ISession.LoadAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

    void ISession.Remove(string key) {
        sessionStorage.Remove(key);
    }

    void ISession.Set(string key, byte[] value) {
        sessionStorage[key] = value;
    }

    bool ISession.TryGetValue(string key, out byte[] value) {
        if (sessionStorage.Keys.Contains(key) && sessionStorage[key] != null) {
            value = (byte[])sessionStorage[key]; //Encoding.UTF8.GetBytes(sessionStorage[key].ToString())
            return true;
        }

        value = null;
        return false;
    }

    public void ClearStorage() {
        sessionStorage.Clear();
    }
}