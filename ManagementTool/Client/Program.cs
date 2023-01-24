using ManagementTool.Client;
using ManagementTool.Client.Utils;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Payloads;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.SessionStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.RootComponents.Add<HeadOutlet>("head::after");

var apiAddress = builder.Configuration.GetValue<string>("ApiBaseUrl");
builder.Services.AddScoped(sp => new HttpClient {
    BaseAddress = new Uri(apiAddress)
});
builder.Services.AddSingleton<StateContainer<UserBasePL>>();
builder.Services.AddSingleton<StateContainer<LoggedUserPayload>>();
builder.Services.AddSingleton<StateContainer<AssignmentWrapperPayload>>();
builder.Services.AddSingleton<StateContainer<ProjectInfoPayload>>();
builder.Services.AddBlazoredSessionStorage();


var app = builder.Build();

await app.RunAsync();