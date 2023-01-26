using System.Net;
using ManagementTool.Server.Services.Roles;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Requests;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase {
    public UsersController(IAuthService authService, IUsersService usersService, IRolesService rolesService) {
        AuthService = authService;
        UsersService = usersService;
        RolesService = rolesService;
    }

    private IAuthService AuthService { get; }
    private IUsersService UsersService { get; }
    private IRolesService RolesService { get; }

    /// <summary>
    /// Endpoint for retrieving all users
    /// This endpoint can be accesses by secretariat, department manager and project manager without limitations.
    /// </summary>
    /// <returns>List of all users known users</returns>
    [HttpGet]
    public IEnumerable<UserBasePL>? GetAllUsers() {
        if (!AuthService.IsAuthorizedToViewUsers()) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var resultUsers = UsersService.GetUsers();
        
        Response.StatusCode = (int)HttpStatusCode.OK;
        

        return resultUsers;
    }

    /// <summary>
    /// This endpoint is assigned to creation of new user
    /// Passed user object should be valid and without any data conflicts.
    /// Otherwise and 4xx status code will be returned
    /// All passed roles and superiors assigns will be compared and updated based on the difference
    /// </summary>
    /// <param name="userRequest">new user object with assigned roles and superiors </param>
    [HttpPost]
    public void CreateUser([FromBody] UserCreationRequest userRequest) {
        if (!AuthService.IsUserAuthorized(RoleType.Secretariat)) {
            //only secretariat can 
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }


        var (response, userId) = UsersService.CreateUser(userRequest.UpdatedUser, userRequest.Pwd);
        userRequest.UpdatedUser.Id = userId;
        if (response != UserCreationResponse.Ok) {
            if (response == UserCreationResponse.UsernameTaken) {
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

    /// <summary>
    /// This endpoint is assigned to updating of existing user
    /// Passed user object should be valid and without any data conflicts.
    /// Otherwise and 4xx status code will be returned
    /// All passed roles and superiors assigns will be compared and updated based on the difference
    /// </summary>
    /// <param name="userRequest">user object with assigned roles and superiors that should be updated</param>
    [HttpPatch]
    public void UpdateUser([FromBody] UserUpdateRequest userRequest) {
        if (!AuthService.IsUserAuthorized(RoleType.Secretariat)) {
            //only secretariat can update users
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        var updateOk = UsersService.UpdateUser(userRequest.UpdatedUser);
        if (updateOk != UserCreationResponse.Ok) {
            if (updateOk == UserCreationResponse.UsernameTaken) {
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

    /// <summary>
    /// Endpoint for deletion of existing user.
    /// Users can be deleted only by secretariat.
    /// If the project is non existent the MethodNotAllowed status code is returned
    /// </summary>
    /// <param name="id">id of the user that should be deleted</param>
    [HttpDelete("{id:long}")]
    public void DeleteUser(long id) {
        if (!AuthService.IsUserAuthorized(RoleType.Secretariat)) {
            //only secretariat can delete users
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        var ok = UsersService.DeleteUser(id);
        if (!ok) {
            Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
        }
    }

    /// <summary>
    /// Endpoint for retrieving of all roles that are assigned to the user
    /// Everyone authorized can access this endpoint
    /// </summary>
    /// <param name="userId">id of the user </param>
    /// <returns></returns>
    [HttpGet("{userId:long}/roles")]
    public IEnumerable<DataModelAssignmentPL<RolePL>>? GetAllUserRoles(long userId) {
        if (!AuthService.IsUserAuthorized(null)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var userRoles = RolesService.GetAllRolesAssigned(userId);
        if (userRoles == null) {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
        }
        else {
            Response.StatusCode = (int)HttpStatusCode.OK;
        }

        return userRoles;
    }

    /// <summary>
    /// Endpoint for retrieving of all superiors currently assigned in the system.
    /// Everyone authorized can access this endpoint
    /// </summary>
    /// <returns>List of all superiors</returns>
    [HttpGet("superiors")]
    public IEnumerable<UserBasePL>? GetAllSuperiors() {
        if (!AuthService.IsUserAuthorized(null)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var users = UsersService.GetAllUsersWithRole(RoleType.Superior);
        Response.StatusCode = (int)HttpStatusCode.OK;

        return users.ToArray();
    }


    /// <summary>
    /// Endpoint for retrieving of all superiors assigned to specified user.
    /// Everyone authorized can access this endpoint
    /// </summary>
    /// <param name="userId">id of the user</param>
    /// <returns></returns>
    [HttpGet("{userId:long}/superiors")]
    public IEnumerable<long>? GetAllUserSuperiors(long userId) {
        if (!AuthService.IsUserAuthorized(null)) {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }

        var superiors = UsersService.GetAllUserSuperiorsIds(userId);
        if (superiors == null) {
            Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            return null;
        }

        Response.StatusCode = (int)HttpStatusCode.OK;

        return superiors;
    }
}