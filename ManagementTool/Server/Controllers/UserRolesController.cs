using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ManagementTool.Shared.Models.ApiModels;


namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserRolesController : ControllerBase {

    public IUserRoleDataService UserRoleDataService { get; }
    public IUserDataService UserDataService { get; }

    public UserRolesController(IUserRoleDataService roleService, IUserDataService userService) {
        UserRoleDataService = roleService;
        UserDataService = userService;
    }

    
    [HttpGet]
    public IEnumerable<Role>? GetAllRoles() {
        if (LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        return UserRoleDataService.GetAllRoles();
    }

    [HttpGet("allUserAssigned/{userId:long}")]
    public List<DataModelAssignment<Role>>? GetAllUserRoles(long userId) {
        if (LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        var allRoles = UserRoleDataService.GetAllNonProjectManRoles();
        
        var userRoles = userId < 1 ? Array.Empty<Role>() : UserRoleDataService.GetUserRolesByUserId(userId).ToArray();

        return (from role in allRoles let assigned = userRoles.Contains(role) select new DataModelAssignment<Role>(assigned, role)).ToList();
    }


    [HttpGet("superiors")]
    public IEnumerable<UserBase>? GetAllSuperiors() {
        if (LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        var supRole = UserRoleDataService.GetRolesByType(ERoleType.Superior).FirstOrDefault();
        if (supRole == null) {
            //role is not found. Not possible without direct db changes
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return Array.Empty<UserBase>();
        }

        

        return UserDataService.GetAllUsersByRole(supRole);
    }


    [HttpGet("superiors/{userId:long}")]
     public IEnumerable<long>? GetAllSuperiors(long userId) {
        if (LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        if (userId < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }

        var supIds = UserDataService.GetAllUserSuperiorsIds(userId);
        return supIds;
    }


    // GET api/<UserRolesController>/5
    [HttpGet("{id}")]
    public IEnumerable<Role>? GetUserRolesById(long userId) {
        if (LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        if (userId < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }
        return UserRoleDataService.GetUserRolesByUserId(userId);
    }
}