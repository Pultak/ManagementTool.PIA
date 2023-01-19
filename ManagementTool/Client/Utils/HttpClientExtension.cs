using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Json;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Client.Utils; 

public static class HttpClientExtension {
    public const string MimeJson = "application/json";
    
    public static Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content) {
        var request = new HttpRequestMessage {
            Method = new HttpMethod("PATCH"),
            RequestUri = new Uri(client.BaseAddress + requestUri),
            Content = content
        };

        return client.SendAsync(request);
    }

    public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string requestUri, Type type, object value) {
        return client.PostAsync(requestUri,
            new ObjectContent(type, value, new JsonMediaTypeFormatter(), MimeJson));
    }

    public static Task<HttpResponseMessage> PutJsonAsync(this HttpClient client, string requestUri, Type type, object value) {
        return client.PutAsync(requestUri,
            new ObjectContent(type, value, new JsonMediaTypeFormatter(), MimeJson));
    }

    public static Task<HttpResponseMessage> PatchJsonAsync(this HttpClient client, string requestUri, Type type, object value) {
        return client.PatchAsync(requestUri,
            new ObjectContent(type, value, new JsonMediaTypeFormatter(), MimeJson));
    }


    public static async Task<(EApiHttpResponse status, T? response)> SendApiGetRequestAsync<T>(this HttpClient client, ILogger logger, string requestUri) {
        try {
            var response = await client.GetFromJsonAsync<T>(requestUri);
            if (response == null) {
                return (EApiHttpResponse.UnknownException, default);
            }

            logger.LogInformation($"SendApiGetRequestAsync -> get sent to {requestUri} was successful!");
            return (EApiHttpResponse.Ok, response);
        }
        catch (HttpRequestException e) {
            return (HandleApiHttpRequestException(e, "SendApiGetRequestAsync", logger), default);
        }
        catch (ArgumentException e) {
            logger.LogError("SendApiGetRequestAsync -> Received invalid data from the API!" + e.Message);
            return (EApiHttpResponse.ArgumentException, default);
        }
        catch (Exception e) {
            logger.LogError("SendApiGetRequestAsync -> Unknown error occurred during authentication! " + e.Message);
            return (EApiHttpResponse.UnknownException, default);
        }

    }


    public static async void SendApiPutRequestAsync<T>(this HttpClient client, ILogger logger, string endpoint, 
        T payload, Action<EApiHttpResponse, bool> resolveResponse) {
        
        var apiResponse = await SendApiPutRequest(client, logger, endpoint, payload);
        resolveResponse(apiResponse, false);
    }



    public static async void SendApiDeleteRequestAsync(this HttpClient client, ILogger logger, string endpoint, 
        Action<EApiHttpResponse, bool> resolveResponse) {
        var apiResponse = await SendApiDeleteRequest(client, logger, endpoint);
        resolveResponse(apiResponse, true);
    }

    public static async void SendApiPatchRequestAsync<T>(this HttpClient client, ILogger logger, string endpoint, 
        T payload, Action<EApiHttpResponse, bool> resolveResponse) {
       
        var apiResponse = await client.SendApiPatchRequest(logger, endpoint, payload);
        resolveResponse(apiResponse, true);
    }
    
    public static async void SendApiPostRequestAsync<T>(this HttpClient client, ILogger logger, string endpoint, 
        T payload, Action<EApiHttpResponse, bool> resolveResponse) {
       
        var apiResponse = await client.SendApiPostRequest(logger, endpoint, payload);
        resolveResponse(apiResponse, false);
    }



    public static async Task<EApiHttpResponse> SendApiPutRequest<T>(this HttpClient client, ILogger logger, string endpoint, T? json) {
        if (json == null) return EApiHttpResponse.InvalidData;
        try {
            var response = await client.PutAsJsonAsync(endpoint, json);
            response.EnsureSuccessStatusCode();
            logger.LogInformation($"SendApiPutRequest -> post sent to {endpoint} was successful!");

            return EApiHttpResponse.Ok;
        }
        catch (HttpRequestException e) {
            return HandleApiHttpRequestException(e, "SendApiPutRequest", logger);
        }
        catch (ArgumentException e) {
            logger.LogError("SendApiPutRequest -> Received invalid data from the API!" + e.Message);
            return EApiHttpResponse.ArgumentException;
        }
        catch (Exception e) {
            logger.LogError("SendApiPutRequest -> Unknown error occurred during authentication!" + e.Message);
            return EApiHttpResponse.UnknownException;
        }
    }


    public static async Task<EApiHttpResponse> SendApiPatchRequest<T>(this HttpClient client, ILogger logger, string endpoint, T? json) {
        if (json == null) return EApiHttpResponse.InvalidData;
        try {
            var response = await client.PatchJsonAsync(endpoint, typeof(T), json);
            response.EnsureSuccessStatusCode();
            logger.LogInformation($"SendApiPatchRequest -> patch sent to {endpoint} was successful!");

            return EApiHttpResponse.Ok;
        }
        catch (HttpRequestException e) {
            return HandleApiHttpRequestException(e, "SendApiPatchRequest", logger);
        }
        catch (ArgumentException e) {
            logger.LogError("SendApiPatchRequest -> Received invalid data from the API!" + e.Message);
            return EApiHttpResponse.ArgumentException;
        }
        catch (Exception e) {
            logger.LogError("SendApiPatchRequest -> Unknown error occurred during authentication! " + e.Message);
            return EApiHttpResponse.UnknownException;
        }
    }



    public static async Task<EApiHttpResponse> SendApiDeleteRequest(this HttpClient client, ILogger logger, string endpoint) {
        try {
            var response = await client.DeleteAsync(endpoint);
            response.EnsureSuccessStatusCode();
            logger.LogInformation($"SendApiDeleteRequest -> delete sent to {endpoint} was successful!");
            return EApiHttpResponse.Ok;
        }
        catch (HttpRequestException e) {
            return HandleApiHttpRequestException(e, "SendApiDeleteRequest", logger);
        }
        catch (ArgumentException e) {
            logger.LogError("SendApiDeleteRequest -> Received invalid data from the API!" + e.Message);
            return EApiHttpResponse.ArgumentException;
        }
        catch (Exception e) {
            logger.LogError("SendApiDeleteRequest -> Unknown error occurred during authentication! " + e.Message);
            return EApiHttpResponse.UnknownException;
        }
    }
    

    public static async Task<EApiHttpResponse> SendApiPostRequest<T>(this HttpClient client, ILogger logger, string endpoint, T json) {
        try {
            var response = await client.PostJsonAsync(endpoint, typeof(T), json!);
            response.EnsureSuccessStatusCode();
            logger.LogInformation($"SendApiDeleteRequest -> delete sent to {endpoint} was successful!");
            return EApiHttpResponse.Ok;
        }
        catch (HttpRequestException e) {
            return HandleApiHttpRequestException(e, "SendApiDeleteRequest", logger);
        }
        catch (ArgumentException e) {
            logger.LogError("SendApiDeleteRequest -> Received invalid data from the API!" + e.Message);
            return EApiHttpResponse.ArgumentException;
        }
        catch (Exception e) {
            logger.LogError("SendApiDeleteRequest -> Unknown error occurred during authentication! " + e.Message);
            return EApiHttpResponse.UnknownException;
        }
    }


    private static EApiHttpResponse HandleApiHttpRequestException(HttpRequestException ex, string functionName, ILogger logger) {
        var aaa = ex.ToString();
        switch (ex.StatusCode) {
            case HttpStatusCode.Conflict:
                logger.LogError(functionName + " -> Api responded that there is data conflict for following object! "+ex.Message);
                return EApiHttpResponse.ConflictFound;

            case HttpStatusCode.UnprocessableEntity:
                logger.LogError(functionName + " -> this client sent bad data that cant be processed by the API! " + ex.Message);
                return EApiHttpResponse.InvalidData;
            case HttpStatusCode.Unauthorized:
                logger.LogError(functionName + " -> this client tried to access API endpoint that he has insufficient rights for!" + ex.Message);
                return EApiHttpResponse.Unauthorized;
            default:
                logger.LogError(functionName + " -> Failure occurred during receiving data from the API! " +
                                ex.StatusCode);
                return EApiHttpResponse.HttpRequestException;

        }
    }

}