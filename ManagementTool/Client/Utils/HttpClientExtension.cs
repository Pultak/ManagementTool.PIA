using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Json;
using ManagementTool.Client.Pages.Shared;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;
using ManagementTool.Shared.Models.Utils;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace ManagementTool.Client.Utils;

public static class HttpClientExtension {


    /// <summary>
    /// Multipurpose Internet Mail Extension of json
    /// </summary>
    public const string MimeJson = "application/json";

    /// <summary>
    /// HTTP patch modifications to the resource, update the given content
    /// </summary>
    /// <param name="client">Client who requests the update</param>
    /// <param name="requestUri">Resource identifier</param>
    /// <param name="content">Content to update</param>
    /// <returns> Asynchronous request task</returns>
    public static Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content) {
        var request = new HttpRequestMessage {
            Method = new HttpMethod("PATCH"),
            RequestUri = new Uri(client.BaseAddress + requestUri),
            Content = content
        };

        return client.SendAsync(request);
    }


    /// <summary>
    /// HTTP Post the given value object wrapped in a json type from the client
    /// </summary>
    /// <param name="client">Client who posts the json</param>
    /// <param name="requestUri">Resource identifier</param>
    /// <param name="type">Type of the object to be posted</param>
    /// <param name="value">Value of the object to be posted</param>
    /// <returns> </returns>
    public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string requestUri, Type type,
        object value) =>
        client.PostAsync(requestUri,
            new ObjectContent(type, value, new JsonMediaTypeFormatter(), MimeJson));


    /// <summary>
    /// Create a new resource or replace the target resource with the given value
    /// </summary>
    /// <param name="client">Client who requests to PUT</param>
    /// <param name="requestUri">Resource identifier</param>
    /// <param name="type">Type of the object to be PUT</param>
    /// <param name="value">Value of the object to be PUT</param>
    /// <returns> </returns>
    public static Task<HttpResponseMessage> PutJsonAsync(this HttpClient client, string requestUri, Type type,
        object value) =>
        client.PutAsync(requestUri,
            new ObjectContent(type, value, new JsonMediaTypeFormatter(), MimeJson));

    /// <summary>
    /// Apply partial modifications to the resource, update the given content as json
    /// </summary>
    /// <param name="client">Client who sends the patch request</param>
    /// <param name="requestUri">Resource identifier</param>
    /// <param name="type">Type of the object to be patched</param>
    /// <param name="value">Value of the object to be patched</param>
    /// <returns> </returns>
    public static Task<HttpResponseMessage> PatchJsonAsync(this HttpClient client, string requestUri, Type type,
        object value) =>
        client.PatchAsync(requestUri,
            new ObjectContent(type, value, new JsonMediaTypeFormatter(), MimeJson));


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Nullable generic type of the response</typeparam>
    /// <param name="client">  </param>
    /// <param name="logger">Where to log</param>
    /// <param name="requestUri">Resource identifier</param>
    /// <returns>  </returns>
    public static async Task<(ApiHttpResponse status, T? response)> SendApiGetRequestAsync<T>(this HttpClient client,
        ILogger logger, string requestUri) {
        try {
            var response = await client.GetFromJsonAsync<T>(requestUri);
            if (response == null) {
                return (ApiHttpResponse.UnknownException, default);
            }

            logger.LogInformation($"SendApiGetRequestAsync -> get sent to {requestUri} was successful!");
            return (ApiHttpResponse.Ok, response);
        }
        catch (HttpRequestException e) {
            return (HandleApiHttpRequestException(e, "SendApiGetRequestAsync", logger), default);
        }
        catch (ArgumentException e) {
            logger.LogError($"SendApiGetRequestAsync -> Received invalid data from the API on {requestUri}!" + e.Message);
            return (ApiHttpResponse.ArgumentException, default);
        }
        catch (Exception e) {
            logger.LogError($"SendApiGetRequestAsync -> Unknown error occurred during authentication on {requestUri}! " + e.Message);
            return (ApiHttpResponse.UnknownException, default);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Generic type of the put request payload</typeparam>
    /// <param name="client">Http client sending put request</param>
    /// <param name="logger">Where to log</param>
    /// <param name="endpoint">Address of the resource</param>
    /// <param name="payload">Data to put</param>
    /// <param name="resolveResponse"> The return method called after api request </param>
    public static async void SendApiPutRequestAsync<T>(this HttpClient client, ILogger logger, string endpoint,
        T payload, Action<ApiHttpResponse, bool> resolveResponse) {
        var apiResponse = await SendApiPutRequest(client, logger, endpoint, payload);
        resolveResponse(apiResponse, false);
    }


    /// <summary>
    ///  
    /// </summary>
    /// <param name="client">Http client to request delete</param>
    /// <param name="logger">Where to log</param>
    /// <param name="endpoint">Address of the resource</param>
    /// <param name="resolveResponse">  The return method called after api request </param>
    public static async void SendApiDeleteRequestAsync(this HttpClient client, ILogger logger, string endpoint,
        Action<ApiHttpResponse, bool> resolveResponse) {
        var apiResponse = await SendApiDeleteRequest(client, logger, endpoint);
        resolveResponse(apiResponse, true);
    }

    /// <summary>
    ///   
    /// </summary>
    /// <typeparam name="T">Generic type of the patch</typeparam>
    /// <param name="client">Http client to request patch</param>
    /// <param name="logger">Where to log</param>
    /// <param name="endpoint">Address of the resource</param>
    /// <param name="payload">Data to patch</param>
    /// <param name="resolveResponse">  The return method called after api request </param>
    public static async void SendApiPatchRequestAsync<T>(this HttpClient client, ILogger logger, string endpoint,
        T payload, Action<ApiHttpResponse, bool> resolveResponse) {
        var apiResponse = await client.SendApiPatchRequest(logger, endpoint, payload);
        resolveResponse(apiResponse, true);
    }

    /// <summary>
    ///   
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="client">Http client to request post</param>
    /// <param name="logger">Where to log</param>
    /// <param name="endpoint">Address of the resource</param>
    /// <param name="payload">Data to post</param>
    /// <param name="resolveResponse">  The return method called after api request </param>
    public static async Task SendApiPostRequestAsync<T>(this HttpClient client, ILogger logger, string endpoint,
        T payload, Action<ApiHttpResponse, bool> resolveResponse) {
        var apiResponse = await client.SendApiPostRequest(logger, endpoint, payload);
        resolveResponse(apiResponse, false);
    }


    /// <summary>
    ///   
    /// </summary>
    /// <typeparam name="T"> type of the serializable object </typeparam>
    /// <param name="client">Http client to request put</param>
    /// <param name="logger">Where to log</param>
    /// <param name="endpoint">Address of the resource</param>
    /// <param name="json">Data to put</param>
    /// <returns>Success/fail status</returns>
    public static async Task<ApiHttpResponse> SendApiPutRequest<T>(this HttpClient client, ILogger logger,
        string endpoint, T? json) {
        if (json == null) {
            return ApiHttpResponse.InvalidData;
        }

        try {
            var response = await client.PutAsJsonAsync(endpoint, json);
            response.EnsureSuccessStatusCode();
            logger.LogInformation($"SendApiPutRequest -> post sent to {endpoint} was successful!");

            return ApiHttpResponse.Ok;
        }
        catch (HttpRequestException e) {
            return HandleApiHttpRequestException(e, "SendApiPutRequest", logger);
        }
        catch (ArgumentException e) {
            logger.LogError("SendApiPutRequest -> Received invalid data from the API!" + e.Message);
            return ApiHttpResponse.ArgumentException;
        }
        catch (Exception e) {
            logger.LogError("SendApiPutRequest -> Unknown error occurred during authentication!" + e.Message);
            return ApiHttpResponse.UnknownException;
        }
    }


    /// <summary>
    ///   
    /// </summary>
    /// <typeparam name="T"> type of the serializable object </typeparam>
    /// <param name="client">Http client to request patch</param>
    /// <param name="logger">Where to log</param>
    /// <param name="endpoint">Address of the resource</param>
    /// <param name="json">Data to patch</param>
    /// <returns>Success/fail status</returns>
    public static async Task<ApiHttpResponse> SendApiPatchRequest<T>(this HttpClient client, ILogger logger,
        string endpoint, T? json) {
        if (json == null) {
            return ApiHttpResponse.InvalidData;
        }

        try {
            var response = await client.PatchJsonAsync(endpoint, typeof(T), json);
            response.EnsureSuccessStatusCode();
            logger.LogInformation($"SendApiPatchRequest -> patch sent to {endpoint} was successful!");

            return ApiHttpResponse.Ok;
        }
        catch (HttpRequestException e) {
            return HandleApiHttpRequestException(e, "SendApiPatchRequest", logger);
        }
        catch (ArgumentException e) {
            logger.LogError("SendApiPatchRequest -> Received invalid data from the API!" + e.Message);
            return ApiHttpResponse.ArgumentException;
        }
        catch (Exception e) {
            logger.LogError("SendApiPatchRequest -> Unknown error occurred during authentication! " + e.Message);
            return ApiHttpResponse.UnknownException;
        }
    }


    /// <summary>
    ///   
    /// </summary>
    /// <param name="client">Http client to request delete</param>
    /// <param name="logger">Where to log</param>
    /// <param name="endpoint">Address of the resource</param>
    /// <returns>Success/fail status</returns>
    public static async Task<ApiHttpResponse> SendApiDeleteRequest(this HttpClient client, ILogger logger,
        string endpoint) {
        try {
            var response = await client.DeleteAsync(endpoint);
            response.EnsureSuccessStatusCode();
            logger.LogInformation($"SendApiDeleteRequest -> delete sent to {endpoint} was successful!");
            return ApiHttpResponse.Ok;
        }
        catch (HttpRequestException e) {
            return HandleApiHttpRequestException(e, "SendApiDeleteRequest", logger);
        }
        catch (ArgumentException e) {
            logger.LogError("SendApiDeleteRequest -> Received invalid data from the API!" + e.Message);
            return ApiHttpResponse.ArgumentException;
        }
        catch (Exception e) {
            logger.LogError("SendApiDeleteRequest -> Unknown error occurred during authentication! " + e.Message);
            return ApiHttpResponse.UnknownException;
        }
    }


    /// <summary>
    ///   
    /// </summary>
    /// <typeparam name="T"> type of the serializable object </typeparam>
    /// <param name="client">Http client to request post</param>
    /// <param name="logger">Where to log</param>
    /// <param name="endpoint">Address of the resource</param>
    /// <param name="json">Data to post</param>
    /// <returns>Success/fail status</returns>
    public static async Task<ApiHttpResponse> SendApiPostRequest<T>(this HttpClient client, ILogger logger,
        string endpoint, T json) {
        try {
            var response = await client.PostJsonAsync(endpoint, typeof(T), json!);
            response.EnsureSuccessStatusCode();
            logger.LogInformation($"SendApiDeleteRequest -> delete sent to {endpoint} was successful!");
            return ApiHttpResponse.Ok;
        }
        catch (HttpRequestException e) {
            return HandleApiHttpRequestException(e, "SendApiDeleteRequest", logger);
        }
        catch (ArgumentException e) {
            logger.LogError("SendApiDeleteRequest -> Received invalid data from the API!" + e.Message);
            return ApiHttpResponse.ArgumentException;
        }
        catch (Exception e) {
            logger.LogError("SendApiDeleteRequest -> Unknown error occurred during authentication! " + e.Message);
            return ApiHttpResponse.UnknownException;
        }
    }


    /// <summary>
    /// Recognize the exception which occurred in the given function specified by functionName,
    /// log the status and return the type of the error
    /// </summary>
    /// <param name="ex">Http request exception to be stratified</param>
    /// <param name="functionName">Where the exception occurred</param>
    /// <param name="logger">Logger for logging the error</param>
    /// <returns>Exception type</returns>
    private static ApiHttpResponse HandleApiHttpRequestException(HttpRequestException ex, string functionName,
        ILogger logger) {
        switch (ex.StatusCode) {
            case HttpStatusCode.Conflict:
                logger.LogError(functionName + " -> Api responded that there is data conflict for following object! " +
                                ex.Message);
                return ApiHttpResponse.ConflictFound;

            case HttpStatusCode.UnprocessableEntity:
                logger.LogError(functionName + " -> this client sent bad data that cant be processed by the API! " +
                                ex.Message);
                return ApiHttpResponse.InvalidData;
            case HttpStatusCode.Unauthorized:
                logger.LogError(functionName +
                                " -> this client tried to access API endpoint that he has insufficient rights for!" +
                                ex.Message);
                return ApiHttpResponse.Unauthorized;
            default:
                logger.LogError(functionName + " -> Failure occurred during receiving data from the API! " +
                                ex.StatusCode);
                return ApiHttpResponse.HttpRequestException;
        }
    }


    /// <summary>
    /// Api call on the login endpoint
    /// </summary>
    /// <param name="http">client to send requests from</param>
    /// <param name="sessionStorage">session manager to store the jwt token on successful login</param>
    /// <param name="logger">Logger to log possible errors</param>
    /// <param name="changeDialogSignature">Return task to change values in UI</param>
    /// <param name="request">Serializable login request </param>
    public static async Task CallLoginApiAsync(this HttpClient http, Blazored.SessionStorage.ISessionStorageService sessionStorage, ILogger logger,
        Action<AuthResponse?> changeDialogSignature, AuthRequest request ) {
        try {
            var response = await http.PostAsJsonAsync("api/auth", request); ;
            response.EnsureSuccessStatusCode();
            var responseBodyString = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseBodyString)) {
                changeDialogSignature(AuthResponse.UnknownResponse);
            }
            else {
                var responsePayload = JsonConvert.DeserializeObject<AuthResponsePayload>(responseBodyString);
                if (!string.IsNullOrEmpty(responsePayload.Token)) {
                    //saving of JWT token
                    await sessionStorage.SetItemAsStringAsync(MainLayout.JWTTokenKey, responsePayload.Token);
                }
                changeDialogSignature(responsePayload.Response);
            }
            return;
        }
        catch (HttpRequestException e) {
            logger.LogError("LoginPressed -> Failure occurred during receiving data from the API! " + e.StatusCode);
        }
        catch (ArgumentException e) {
            logger.LogError("LoginPressed -> Received invalid data from the API!" + e.Message);
        }
        catch (Exception e) {
            logger.LogError("LoginPressed -> Unknown error occurred during authentication!" + e.Message);
        }
        changeDialogSignature(AuthResponse.UnknownResponse);
    }

}