using Microsoft.AspNetCore.Http;

namespace ManagementTool.ServerTests.MoqModels;

public class MoqHttpResponse : HttpResponse {
    public override HttpContext HttpContext { get; }
    public override int StatusCode { get; set; }
    public override IHeaderDictionary Headers { get; }
    public override Stream Body { get; set; }
    public override long? ContentLength { get; set; }
    public override string ContentType { get; set; } = string.Empty;
    public override IResponseCookies Cookies { get; } 
    public override bool HasStarted { get; } = false;

    public override void OnStarting(Func<object, Task> callback, object state) {
        throw new NotImplementedException();
    }

    public override void OnCompleted(Func<object, Task> callback, object state) {
        throw new NotImplementedException();
    }

    public override void Redirect(string location, bool permanent) {
        throw new NotImplementedException();
    }
}