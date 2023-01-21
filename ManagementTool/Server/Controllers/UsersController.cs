using System.Net;
using System.Security.Cryptography;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Services.Roles;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Api.Payloads;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;
using Microsoft.AspNetCore.Mvc;
using ManagementTool.Shared.Models.Api.Requests;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase {

    private IAuthService AuthService { get; }
    private IUsersService UsersService { get; }
    private IRolesService RolesService { get; }

    public UsersController(IAuthService authService, IUsersService usersService, IRolesService rolesService) {
        AuthService = authService;
        UsersService = usersService;
        RolesService = rolesService;
    }
    
    [HttpGet]
    public IEnumerable<UserBase>? GetAllUsers() {
        if (!AuthService.IsAuthorizedToViewUsers()) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        var resultUsers = UsersService.GetUsers();
        if (resultUsers.Any()) {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.NoContent;
        }

        return resultUsers;
    }


    //todo changed
    [HttpPost]
    public void CreateUser([FromBody] UserUpdateRequest<User> userRequest) {
        if (!AuthService.IsUserAuthorized(ERoleType.Secretariat)) {
            //only secretariat can 
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }


        var response = UsersService.CreateUser(userRequest.UpdatedUser);
        if (response != EUserCreationResponse.Ok) {
            if (response == EUserCreationResponse.UsernameTaken) {
                Response.StatusCode = (int)HttpStatusCode.Conflict;
            }
            else {
                Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            }
            return;
        }

        RolesService.UpdateUserRoleAssignments(userRequest.AssignedRoles, userRequest.UpdatedUser.Id);
        UsersService.UpdateUserSuperiorAssignments(userRequest.Superiors, userRequest.UpdatedUser);

        Response.StatusCode = (int)HttpStatusCode.OK;
    }

    //todo changed
    [HttpPatch]
    public void UpdateUser([FromBody] UserUpdateRequest<UserBase> userRequest) {
        if (!AuthService.IsUserAuthorized(ERoleType.Secretariat)) {
            //only secretariat can update users
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        var updateOk = UsersService.UpdateUser(userRequest.UpdatedUser);
        if (updateOk != EUserCreationResponse.Ok) {
            if (updateOk == EUserCreationResponse.UsernameTaken) {
                Response.StatusCode = (int)HttpStatusCode.Conflict;
            }
            else {
                Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            }
        }

        RolesService.UpdateUserRoleAssignments(userRequest.AssignedRoles, userRequest.UpdatedUser.Id);
        UsersService.UpdateUserSuperiorAssignments(userRequest.Superiors, userRequest.UpdatedUser);
        Response.StatusCode = (int)HttpStatusCode.OK;
    }

    [HttpDelete("{id}")]
    public void DeleteUser(long id) {
        if (!AuthService.IsUserAuthorized(ERoleType.Secretariat)) {
            //only secretariat can delete users
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }
        var ok = UsersService.DeleteUser(id);
        if (!ok) {
            Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
        }
    }


    
    //todo changed from roles controller
    [HttpGet("{userId:long}/roles")]
    public IEnumerable<DataModelAssignment<Role>>? GetAllUserRoles(long userId) {
        if (!AuthService.IsUserAuthorized(null)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        var userRoles = RolesService.GetAllRolesAssigned(userId);
        if (userRoles == null) {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        }
        else if (!userRoles.Any()) {
            Response.StatusCode = (int)HttpStatusCode.NoContent;
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }
        
        return userRoles;
    }

    //todo changed from roles controller
    [HttpGet("superiors")]
    public IEnumerable<UserBase>? GetAllSuperiors() {
        if (!AuthService.IsUserAuthorized(null)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var users = UsersService.GetAllUsersWithRole(ERoleType.Superior);
        if (users.Any()) {
            Response.StatusCode = (int)HttpStatusCode.NoContent;

        }
        else {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }
        return users.ToArray();
    }

    //todo changed from roles controller
    [HttpGet("{userId:long}/superiors")]
     public IEnumerable<long>? GetAllUserSuperiors(long userId) {
        if (!AuthService.IsUserAuthorized(null)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var superiors = UsersService.GetAllUserSuperiorsIds(userId);
        if (superiors == null) {
            Response.StatusCode = (int) HttpStatusCode.UnprocessableEntity;
            return null;
        }
        if (!superiors.Any()) {
            Response.StatusCode = (int) HttpStatusCode.NoContent;
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }

        return superiors;
    }
}