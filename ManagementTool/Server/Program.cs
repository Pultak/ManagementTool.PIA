using ManagementTool.Server.Repository;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using Microsoft.EntityFrameworkCore;
using ManagementTool.Server.Services.Assignments;
using ManagementTool.Server.Services.Roles;

namespace ManagementTool.Server;

internal class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        Configure(builder.Services, builder.Configuration);
        ConfigureRepositories(builder.Services, builder.Configuration);
        ConfigureServices(builder.Services);

        var app = builder.Build();
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) {
            app.UseWebAssemblyDebugging();
        }
        else {
            app.UseExceptionHandler("/Error");
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


    private static void ConfigureRepositories(IServiceCollection services, IConfiguration configuration) {
        var dbConnectionString = configuration.GetValue<string>("DBPosgreSQL");

        services.AddDbContext<ManToolDbContext>(x =>
            x.UseNpgsql(dbConnectionString)
        );

        // Add services to the container.
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IUserRoleRepository, UserRoleRepository>();
        services.AddTransient<IAssignmentRepository, AssignmentRepository>();
        services.AddTransient<IProjectRepository, ProjectRepository>();
    }

    private static void ConfigureServices(IServiceCollection services) {

        services.AddTransient<IAssignmentRepository, AssignmentRepository>();
        services.AddTransient<IWorkloadService, WorkloadService>();
        services.AddTransient<IProjectsService, ProjectsService>();
        services.AddTransient<IRolesService, RolesService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IUsersService, UsersService>();
    }

    private static void Configure(IServiceCollection services, IConfiguration configuration) {

        services.AddAutoMapper(typeof(Program));
        services.AddControllersWithViews();
        services.AddRazorPages();
        /*var keysDirectory = configuration.GetValue<string>("KeysFolder");
        services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
            .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration{
                EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
            });*/
        services.AddHttpContextAccessor();
        services.AddDistributedMemoryCache();
        services.AddSession(options => {
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        
    }
}

