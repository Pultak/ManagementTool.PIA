using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared;
using ManagementTool.Shared.Models.Database;
using Microsoft.AspNetCore.Mvc;

namespace ManagementTool.Server.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public IAssignmentDataService AssignmentDataService { get; }
        public IUserDataService UserDataService { get; }
        public IUserRoleDataService UserRoleDataService { get; }
        public IProjectDataService ProjectDataService { get; }


        public WeatherForecastController(ILogger<WeatherForecastController> logger, IAssignmentDataService assignmentService,
            IUserDataService userService, IUserRoleDataService userRoleService, IProjectDataService projectService) {

            AssignmentDataService = assignmentService;
            UserDataService = userService;
            UserRoleDataService = userRoleService;
            ProjectDataService = projectService;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get() {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}