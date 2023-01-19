using ManagementTool.Client;
using ManagementTool.Shared.Models.ApiModels;
using ManagementTool.Shared.Models.AppState;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Login;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<StateContainer<UserBase>>();
builder.Services.AddSingleton<StateContainer<Project>>();
builder.Services.AddSingleton<StateContainer<LoggedUserPayload>>();
builder.Services.AddSingleton<StateContainer<AssignmentWrapper>>();

await builder.Build().RunAsync();
