using System.Net.Mail;
using System.Text.RegularExpressions;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManagementTool.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase {

    public static Regex PasswordRegex = new("^.*(?=.{8,})(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!*@#$%^&+=]).*$");
    public static Regex FullNameRegex = new("\\w+, [\\w]+[ \\w+]*");


    public IUserDataService UserDataService { get; }
    public IProjectDataService ProjectDataService { get; }

    public UsersController(IUserDataService userService, IProjectDataService projectService) {
        UserDataService = userService;
        ProjectDataService = projectService;
    }

    // GET: api/<UsersController>
    [HttpGet]
    public IEnumerable<User> GetAllUsers() {
        //todo authorized?
        return UserDataService.GetAllUsers();
    }

    // GET api/<UsersController>/5
    [HttpGet("{id:long}")]
    public IActionResult GetUserById(long id) {

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

    // POST api/<UsersController>
    [HttpPut]
    public IActionResult CreateUser([FromBody] User user) {

        if (user == null) {
            return BadRequest(EUserCreationResponse.EmptyUser);
        }

        var valResult = ValidateNewUser(user);
        if (valResult != EUserCreationResponse.Ok) {
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

    /* todo delete not specified in assignment
    // DELETE api/<UsersController>/5
    [HttpDelete("{id}")]
    public void Delete(int id) {

    }*/



    // PUT api/<UsersController>/assignProject/
    [HttpPut("assignUser/{idUser:long}/{idProject:long}")]
    public IActionResult AssignUserToProject(long idUser, long idProject) {

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




    private EUserCreationResponse ValidateNewUser(User user){

        if (user.Username.Length is < 4 or > 32) {
            return EUserCreationResponse.InvalidUsername;
        }

        if (user.Username.Any(char.IsWhiteSpace)) {
            return EUserCreationResponse.InvalidUsername;
        }

        if (user.Pwd.Length is < 4 or < 32) {
            //todo validation on regex PasswordRegex

            return EUserCreationResponse.InvalidPassword;
        }

        if (!FullNameRegex.IsMatch(user.FullName)) {
            //todo czech characters might be a problem for regex
            return EUserCreationResponse.InvalidFullName;

        }
        //todo workplace check? if(user.PrimaryWorkplace)

        try {
            //validation of email address
            MailAddress parsedAddress = new(user.EmailAddress);
        }
        catch (FormatException e) {
            return EUserCreationResponse.InvalidEmail;
        }

        return EUserCreationResponse.Ok;
    }

    private EUserCreationResponse CheckUserDataConflicts(User user) {
        var allUsers = UserDataService.GetAllUsers();
        if (allUsers.Any(existingUser => existingUser.Username.Equals(user.Username))) {
            return EUserCreationResponse.UsernameTaken;
        }
        return EUserCreationResponse.Ok;
    }
}