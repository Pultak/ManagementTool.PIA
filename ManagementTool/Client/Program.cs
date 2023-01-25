using ManagementTool.Client;
using ManagementTool.Client.Utils;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.SessionStorage;

// Create default wasm application
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.RootComponents.Add<HeadOutlet>("head::after");

// Create the client and with the app base address
var apiAddress = builder.Configuration.GetValue<string>("ApiBaseUrl");

var timeString = builder.Configuration["KeepAliveTime"];
//3600 is http max
if (!int.TryParse(timeString, out var time) || time > 3600) {
    time = 3600;
}
builder.Services.AddSingleton(sp => {
    var client = new HttpClient {
        BaseAddress = new Uri(apiAddress),
    };

    client.DefaultRequestHeaders.Add("Connection", "keep-alive");
    client.DefaultRequestHeaders.Add("Keep-Alive", $"timeout={time} max={time}");
    return client;
});

// Add useful services
builder.Services.AddSingleton<StateContainer<UserBasePL>>();
builder.Services.AddSingleton<StateContainer<LoggedUserPayload>>();
builder.Services.AddSingleton<StateContainer<AssignmentWrapperPayload>>();
builder.Services.AddSingleton<StateContainer<ProjectInfoPayload>>();
builder.Services.AddBlazoredSessionStorage();

// Build and run the wasm
var app = builder.Build();

await app.RunAsync();