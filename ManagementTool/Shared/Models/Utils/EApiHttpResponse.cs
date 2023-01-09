namespace ManagementTool.Shared.Models.Utils; 

public enum EApiHttpResponse {
    Ok,
    HttpRequestException,
    ArgumentException,
    UnknownException,
    InvalidData,
    ConflictFound,
    HttpResponseException

}