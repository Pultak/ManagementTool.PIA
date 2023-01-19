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

    public IUserRoleRepository UserRoleRepository { get; }
    public IUserRepository UserRepository { get; }

    public UserRolesController(IUserRoleRepository roleService, IUserRepository userService) {
        UserRoleRepository = roleService;
        UserRepository = userService;
    }

    
    [HttpGet]
    public IEnumerable<Role>? GetAllRoles() {
        if (!LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        Response.StatusCode = (int)HttpStatusCode.OK;
        return UserRoleRepository.GetAllRoles();
    }

    [HttpGet("allUserAssigned/{userId:long}")]
    public List<DataModelAssignment<Role>>? GetAllUserRoles(long userId) {
        if (!LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        var allRoles = UserRoleRepository.GetAllNonProjectManRoles();
        
        var userRoles = userId < 1 ? Array.Empty<Role>() : UserRoleRepository.GetUserRolesByUserId(userId).ToArray();

        Response.StatusCode = (int)HttpStatusCode.OK;
        var result = (from role in allRoles
            let assigned = userRoles.Any(x => x.Id == role.Id)
            select new DataModelAssignment<Role>(assigned, role)).ToList();
        return result;
    }


    [HttpGet("superiors")]
    public UserBase[]? GetAllSuperiors() {
        if (!LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        var supRole = UserRoleRepository.GetRolesByType(ERoleType.Superior).FirstOrDefault();
        if (supRole == null) {
            //role is not found. Not possible without direct db changes
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return Array.Empty<UserBase>();
        }
        
        Response.StatusCode = (int)HttpStatusCode.OK;
        var result = UserRepository.GetAllUsersByRole(supRole);
        return result.ToArray();
    }


    [HttpGet("superiors/{userId:long}")]
     public IEnumerable<long>? GetAllSuperiors(long userId) {
        if (!LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        if (userId < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }

        var supIds = UserRepository.GetAllUserSuperiorsIds(userId);
        Response.StatusCode = (int)HttpStatusCode.OK;
        return supIds;
    }


    // GET api/<UserRolesController>/5
    [HttpGet("{id}")]
    public IEnumerable<Role>? GetUserRolesById(long userId) {
        if (!LoginController.IsUserAuthorized(null, HttpContext.Session)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        if (userId < 1) {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return null;
        }
        Response.StatusCode = (int)HttpStatusCode.OK;
        return UserRoleRepository.GetUserRolesByUserId(userId);
    }
}