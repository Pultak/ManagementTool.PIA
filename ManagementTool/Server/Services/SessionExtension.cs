using Newtonsoft.Json;

namespace ManagementTool.Server.Services;

/// <summary>
/// Utils extension for the HttpSession. Easier object object mapping
/// </summary>
public static class SessionExtensions {
    public static void SetObject(this ISession session, string key, object value) {
        var serialized = JsonConvert.SerializeObject(value);
        session.SetString(key, serialized);
    }

    public static T? GetObject<T>(this ISession session, string key) {
        var value = session.GetString(key);
        return value == null ? default : JsonConvert.DeserializeObject<T>(value);
    }
}
