using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Database;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserRolesController : ControllerBase {

    public IUserRoleDataService UserRoleDataService { get; }

    public UserRolesController(IUserRoleDataService roleService) {
        UserRoleDataService = roleService;
    }



    // GET: api/<UserRolesController>
    [HttpGet]
    public IEnumerable<Role> GetAllRoles() {
        //todo
        return UserRoleDataService.GetAllRoles();
    }

    // GET api/<UserRolesController>/5
    [HttpGet("{id}")]
    public IEnumerable<Role> GetUserRolesById(long userId) {
        //todo checks etc.
        return UserRoleDataService.GetUserRolesById(userId);
    }
}