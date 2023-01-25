using ManagementTool.Server.Models;
using ManagementTool.Server.Repository;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Server.Services.Assignments;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Roles;
using ManagementTool.Server.Services.Users;
using Microsoft.EntityFrameworkCore;

namespace ManagementTool.Server;

internal class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        Configure(builder.Services, builder.Configuration);
        //configure all repositories
        ConfigureRepositories(builder.Services, builder.Configuration);
        //configure all used services around the backend
        ConfigureServices(builder.Services);

        var app = builder.Build();
        //to allow inserting of all date times to database
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
       
        app.UseCors();
        

        app.UseSession();
        app.UseAuthorization();
        
        app.MapRazorPages();
        app.MapFallbackToFile("index.html");
        

        app.MapControllers();

        app.Run();
    }


    private static void ConfigureRepositories(IServiceCollection services, IConfiguration configuration) {
        var dbConnectionString = configuration.GetValue<string>("DBPosgreSQL");

        services.AddDbContext<ManToolDbContext>(x =>
            x.UseNpgsql(dbConnectionString)
        );

        // Add repositories to the container.
        //Transient  => a new instance is provided to every controller and every service
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IUserRoleRepository, UserRoleRepository>();
        services.AddTransient<IAssignmentRepository, AssignmentRepository>();
        services.AddTransient<IProjectRepository, ProjectRepository>();
    }

    private static void ConfigureServices(IServiceCollection services) {
        //Transient  => a new instance is provided to every controller and every service
        services.AddTransient<IAssignmentService, AssignmentService>();
        services.AddTransient<IWorkloadService, WorkloadService>();
        services.AddTransient<IProjectsService, ProjectsService>();
        services.AddTransient<IRolesService, RolesService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IUsersService, UsersService>();
        services.AddSingleton<TokenMap>();
    }

    private static void Configure(IServiceCollection services, IConfiguration configuration) {
        services.AddCors(options => {
            //allow any request from outside of the server container
            options.AddDefaultPolicy(builder => 
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            );
        });
        services.AddAutoMapper(typeof(Program));
        services.AddControllersWithViews();
        services.AddRazorPages();
        services.AddHttpContextAccessor();
        services.AddDistributedMemoryCache();

        //timeout to kick user session
        var timeout = double.Parse(configuration["IdleTimeout"]);
        services.AddSession(options => {
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.IdleTimeout = TimeSpan.FromHours(timeout);
        });




    }
}

