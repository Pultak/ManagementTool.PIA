using System.Text;
using ManagementTool.Server.Repository;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Server.Services.Assignments;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Roles;
using ManagementTool.Server.Services.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Swashbuckle.AspNetCore.Filters;

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

        //app.UseBlazorFrameworkFiles();
        //app.UseStaticFiles();

        app.UseCors();
        app.UseRouting();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();

        //app.MapRazorPages();
        //app.MapFallbackToFile("index.html");
        
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
    }

    private static void Configure(IServiceCollection services, IConfiguration configuration) {
        services.AddEndpointsApiExplorer();

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
        //services.AddRazorPages();
        services.AddHttpContextAccessor();
        services.AddSwaggerGen(options => {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme {
                Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });

            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });

        var securitySection = configuration.GetSection("Security");
        var jwtKey = securitySection["JWTTokenKey"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                        .GetBytes(jwtKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
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

