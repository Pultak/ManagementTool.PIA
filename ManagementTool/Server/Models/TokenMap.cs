using System.Collections.Concurrent;
using ManagementTool.Server.Models.Business;

namespace ManagementTool.Server.Models; 

/// <summary>
/// Singleton class holding sessions information accessible by generated token 
/// </summary>
public class TokenMap {
    public ConcurrentDictionary<string, SessionInfo> UserMap = new();
}