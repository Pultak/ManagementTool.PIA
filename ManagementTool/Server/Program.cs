using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Server.Services;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using Microsoft.EntityFrameworkCore;

namespace ManagementTool.Server;

internal class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        Configure(builder.Services, builder.Configuration);
        ConfigureServices(builder.Services);

        var app = builder.Build();
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) {
            app.UseWebAssemblyDebugging();
        }
        else {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();
        //after use routing and before useEndpoints
        app.UseSession();
        app.UseAuthorization();


        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        app.Run();
    }


    private static void ConfigureServices(IServiceCollection services) {
        // Add services to the container.
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IUserRoleRepository, UserRoleRepository>();
        services.AddTransient<IAssignmentRepository, AssignmentRepository>();
        services.AddTransient<IProjectRepository, ProjectRepository>();
    }

    private static void Configure(IServiceCollection services, IConfiguration configuration) {

        services.AddControllersWithViews();
        services.AddRazorPages();


        services.AddDistributedMemoryCache();
        services.AddSession(options => {
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        var dbConnectionString = configuration.GetValue<string>("DBPosgreSQL");

        services.AddDbContext<ManToolDbContext>(x =>
            x.UseNpgsql(dbConnectionString)
        );
    }
}

