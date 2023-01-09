using System.Net.Mail;
using System.Text.RegularExpressions;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using ManagementTool.Shared.Utils;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase {


    public IUserDataService UserDataService { get; }
    public IProjectDataService ProjectDataService { get; }

    public UsersController(IUserDataService userService, IProjectDataService projectService) {
        UserDataService = userService;
        ProjectDataService = projectService;
    }
    
    [HttpGet]
    public IEnumerable<User> GetAllUsers() {
        //todo authorized?
        return UserDataService.GetAllUsers();
    }
    
    [HttpGet("{id:long}")]
    public ActionResult GetUserById(long id) {

        if (id < 0) {
            return BadRequest();
        }

        var user = UserDataService.GetUserById(id);

        if (user == null) {
            return NotFound();
        }

        user.Pwd = "NiceTry:-)";

        return Ok(user);
    }
    
    [HttpPut]
    public ActionResult CreateUser([FromBody] User user) {

        if (user == null) {
            return BadRequest(EUserCreationResponse.EmptyUser);
        }

        var valResult = UserUtils.ValidateUser(user);
        if (valResult != EUserCreationResponse.Ok || UserUtils.ValidatePassword(user.Pwd)) {
            return UnprocessableEntity(valResult);
        }

        valResult = CheckUserDataConflicts(user);
        if (valResult != EUserCreationResponse.Ok) {
            return Conflict(valResult);
        }


        var userId = UserDataService.AddUser(user);

        if (userId < 0) {
            //todo saving failed!
            return StatusCode(500);
        }

        return Ok(userId);
    }


    [HttpPatch("update")]
    public void UpdateUser([FromBody] User user) {
        if (user == null) {
            throw new BadHttpRequestException("The body cant be empty!");
        }

        var valResult = UserUtils.ValidateUser(user);
        if (valResult == EUserCreationResponse.Ok) {
            UserDataService.UpdateUser(user);
        }
        else {
            throw new BadHttpRequestException("Validation of updated user failed! Reason: " + valResult);
        }
    }

    [HttpDelete("{id}")]
    public void Delete(int id) {
        //todo
    }



    // PUT api/<UsersController>/assignProject/
    [HttpPut("assignUser/{idUser:long}/{idProject:long}")]
    public ActionResult AssignUserToProject(long idUser, long idProject) {

        if (idUser < 0 && idProject < 0) {
            return BadRequest();
        }

        var user = UserDataService.GetUserById(idUser);
        if (user == null) {
            return NotFound();
        }

        var project = ProjectDataService.GetProjectById(idProject);
        if (project == null) {
            return NotFound();
        }
        
        var ok = UserDataService.AssignUserToProject(user, project);
        if (!ok) {
            return StatusCode(500);
        }

        return Ok();
    }



    private EUserCreationResponse CheckUserDataConflicts(User user) {
        var allUsers = UserDataService.GetAllUsers();
        if (allUsers.Any(existingUser => existingUser.Username.Equals(user.Username))) {
            return EUserCreationResponse.UsernameTaken;
        }
        return EUserCreationResponse.Ok;
    }
}