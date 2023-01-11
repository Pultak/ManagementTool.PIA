using System.Net;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using System.Net.Http.Formatting;
using System.Web.Http;
using Microsoft.Extensions.Logging;
using System.Web.Http.Results;

namespace ManagementTool.Shared.Utils; 

public static class WebUtils {
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
       
        var apiResponse = await WebUtils.SendApiPatchRequest(client, logger, endpoint, payload);
        resolveResponse(apiResponse, true);
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
            logger.LogError("SendApiPutRequest -> Received invalid data from the API!");
            return EApiHttpResponse.ArgumentException;
        }
        catch (Exception e) {
            logger.LogError("SendApiPutRequest -> Unknown error occurred during authentication!");
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
            logger.LogError("SendApiPatchRequest -> Received invalid data from the API!");
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
            logger.LogError("SendApiDeleteRequest -> Received invalid data from the API!");
            return EApiHttpResponse.ArgumentException;
        }
        catch (Exception e) {
            logger.LogError("SendApiDeleteRequest -> Unknown error occurred during authentication! " + e.Message);
            return EApiHttpResponse.UnknownException;
        }
    }


    private static EApiHttpResponse HandleApiHttpRequestException(HttpRequestException ex, string functionName, ILogger logger) {

        switch (ex.StatusCode) {
            case HttpStatusCode.Conflict:
                logger.LogError(functionName + " -> Api responded that there is data conflict for following object!");
                return EApiHttpResponse.ConflictFound;

            case HttpStatusCode.UnprocessableEntity:
                logger.LogError(functionName + " -> this client sent bad data that cant be processed by the API!");
                return EApiHttpResponse.InvalidData;
            default:

                logger.LogError(functionName + " -> Failure occurred during receiving data from the API! " +
                                ex.StatusCode);
                return EApiHttpResponse.HttpRequestException;

        }
    }

}