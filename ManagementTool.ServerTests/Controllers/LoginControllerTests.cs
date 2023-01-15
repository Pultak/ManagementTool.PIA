using ManagementTool.Server.Controllers;
using ManagementTool.Server.Services;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.ServerTests.MoqModels;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace ManagementTool.ServerTests.Controllers;

public class LoginControllerTests {

    public static Role[] SuperiorRole = {
        new(1, "Superior", ERoleType.Superior, null)
    };

    public static Role[] DepartmentManagerRole = {
        new(1, "DepartmentManager", ERoleType.DepartmentManager, null)
    };

    public static Role[] SecretariatRole = {
        new(1, "Secretariat", ERoleType.Secretariat, null)
    };
    
    public static Role[] ProjectManagerRoles = {
        new(1, "TEST1ProjectManager", ERoleType.ProjectManager, 1),
        new(2, "TEST2ProjectManager", ERoleType.ProjectManager, 2)
    };

    private LoginController _controller;

    private Mock<HttpContext> _mockHttpContext;
    private HttpResponse _mockHttpResponse;
    private MockHttpSession _mockHttpSession;

    private Mock<IProjectDataService> _mockProjectService;
    private Mock<IUserRoleDataService> _mockRoleService;

    private Mock<IUserDataService> _mockUserService;



    public User? FullDbUser1;
    public User? FullDbUser2;
    public byte[]? FullDbUserSalt;

    public const string FullDbUser1Pwd = "TestPWD1";
    public const string FullDbUser1Username = "Username1";
    public const string FullDbUser2Username = "Username2";

    [OneTimeSetUp]
    public void OneTimeSetUp() {

        FullDbUserSalt = UsersController.GenerateSalt();
        FullDbUser1 = new User {
            Id = 1,
            EmailAddress = "test1@addr.cz",
            FullName = "FullName1",
            PrimaryWorkplace = $"Workplace1",
            Pwd = LoginController.HashPwd(FullDbUser1Pwd, FullDbUserSalt),
            Username = FullDbUser1Username,
            PwdInit = true,
            Salt = Convert.ToBase64String(FullDbUserSalt)
        };
        FullDbUser2 = new User {
            Id = 2,
            EmailAddress = "test2@addr.cz",
            FullName = "FullName2",
            PrimaryWorkplace = "Workplace2",
            Pwd = LoginController.HashPwd(FullDbUser1Pwd, FullDbUserSalt),
            Username = FullDbUser2Username,
            PwdInit = true,
            Salt = Convert.ToBase64String(FullDbUserSalt)
        };


        _mockHttpContext = new Mock<HttpContext>();

        _mockHttpResponse = new MoqHttpResponse();
        _mockHttpSession = new MockHttpSession();

        _mockHttpContext.Setup(s => s.Response).Returns(_mockHttpResponse);
        _mockHttpContext.Setup(s => s.Session).Returns(_mockHttpSession);

        //setup userService
        _mockUserService = new Mock<IUserDataService>();
        _mockRoleService = new Mock<IUserRoleDataService>();

        _controller = new LoginController(_mockUserService.Object, _mockRoleService.Object) {
            ControllerContext = {
                HttpContext = _mockHttpContext.Object
            }
        };
    }

    
    [SetUp]
    public void Setup() {
        _mockUserService.Reset();
        _mockUserService.Setup(x => x.GetUserByName(FullDbUser1Username)).Returns(FullDbUser1);
        _mockUserService.Setup(x => x.GetUserByName(FullDbUser2Username)).Returns(FullDbUser2);
        _mockUserService.Setup(x => x.GetUserById(1)).Returns(FullDbUser1);
        _mockUserService.Setup(x => x.GetUserById(2)).Returns(FullDbUser2);
        _mockUserService.Setup(x => x.UpdateUserPwd(It.IsAny<User>())).Returns(true);
        
        _mockRoleService.Reset();
        _mockRoleService.Setup(x => x.GetUserRolesByUserId(1)).Returns(SecretariatRole);
        _mockRoleService.Setup(x => x.GetUserRolesByUserId(2)).Returns(SecretariatRole);

        _mockHttpSession.ClearStorage();
    }
    
    [Test]
    [TestCase("", "")]
    [TestCase("user", "")]
    public void Login_EmptyCredentials_BadRequest(string username, string pwd) {

        AuthPayload authPayload = new(username, pwd);

        var response = _controller.Login(authPayload);

        Assert.That(response, Is.EqualTo(EAuthResponse.BadRequest));
        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
    }

    
    [Test]
    public void Login_ValidData_AlreadyLoggedIn() {
        SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        AuthPayload authPayload = new("username", "pwd");

        var response = _controller.Login(authPayload);

        Assert.That(response, Is.EqualTo(EAuthResponse.AlreadyLoggedIn));
        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
    }

    
    
    [Test]
    public void Login_WrongUsernamePassed_UnknownUser() {

        AuthPayload authPayload = new("pepa", "pwd");

        var response = _controller.Login(authPayload);

        Assert.That(response, Is.EqualTo(EAuthResponse.UnknownUser));
    }


    [Test]
    public void Login_WrongPwdPassed_WrongPassword() {

        AuthPayload authPayload = new("Username1", "pwd");

        var response = _controller.Login(authPayload);

        Assert.That(response, Is.EqualTo(EAuthResponse.WrongPassword));
    }

    
    [Test]
    public void Login_ValidCredentials_OkLoggedIn() {

        AuthPayload authPayload = new(FullDbUser2Username, FullDbUser1Pwd);

        var response = _controller.Login(authPayload);

        Assert.That(response, Is.EqualTo(EAuthResponse.Success));
        Assert.That(_mockHttpSession.GetInt32(LoginController.UserIdKey), Is.EqualTo(FullDbUser2!.Id));
        Assert.That(_mockHttpSession.GetString(LoginController.UsernameKey), Is.EqualTo(FullDbUser2!.Username));
        Assert.That(_mockHttpSession.GetString(LoginController.UserFullNameKey), Is.EqualTo(FullDbUser2!.FullName));

        var roles = _mockHttpSession.GetObject<Role[]>(LoginController.UserRolesKey);

        Assert.That(roles!.Length, Is.EqualTo(SecretariatRole.Length));
        Assert.That(roles![0].Name, Is.EqualTo(SecretariatRole[0].Name));
        Assert.That(roles![0].Id, Is.EqualTo(SecretariatRole[0].Id));
        Assert.That(_mockHttpSession.GetInt32(LoginController.UserHasInitPwdKey) != 0, Is.EqualTo(FullDbUser2!.PwdInit));
    }

    
    
    [Test]
    public void Logout_NotLoggedIn_UnknownUser() {
        var response = _controller.Logout();

        Assert.That(response, Is.EqualTo(EAuthResponse.UnknownUser));
    }
    
    [Test]
    public void Logout_LoggedIn_Success() {
        SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        var response = _controller.Logout();

        Assert.That(response, Is.EqualTo(EAuthResponse.Success));
        Assert.That(_mockHttpSession.GetInt32(LoginController.UserIdKey), Is.Null);
    }

    
    
    [Test]
    public void GetLoggedInUser_LoggedIn_Success() {
        SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        var response = _controller.GetLoggedInUser();

        Assert.That(response.UserName, Is.EqualTo("Username"));
        Assert.That(response.FullName, Is.EqualTo("FullName"));
        Assert.That(response.Roles!.Length, Is.EqualTo(SecretariatRole.Length));
        Assert.That(response.Roles![0].Name, Is.EqualTo(SecretariatRole[0].Name));
        Assert.That(response.Roles![0].Id, Is.EqualTo(SecretariatRole[0].Id));
        Assert.That(response.HasInitPwd, Is.True);
    }

    
    
    [Test]
    public void LoggedInUserChangePwd_NotLoggedIn_Unauthorized() {
        _controller.LoggedInUserChangePwd("");

        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.Unauthorized));
    }

    
    
    [Test]
    public void LoggedInUserChangePwd_LoggedInUnknownUser_NotFound() {
        SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        //set id for non existent user
        _mockHttpSession.SetInt32(LoginController.UserIdKey, 10);
        
        _controller.LoggedInUserChangePwd("");

        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
    }

    
    
    [Test]
    public void LoggedInUserChangePwd_LoggedInNotValidPwd_BadRequest() {
        SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        
        _controller.LoggedInUserChangePwd("");

        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
    }
    
    
    [Test]
    public void LoggedInUserChangePwd_LoggedInValidPwd_OK() {
        SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        var newPwd = "Abc12345";

        var hashedPwd = LoginController.HashPwd(newPwd, Convert.FromBase64String(FullDbUser1.Salt));

        _controller.LoggedInUserChangePwd(newPwd);

        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        _mockUserService.Verify(x => x.UpdateUserPwd(It.Is<User>(x => x.Pwd.Equals(hashedPwd))), Times.Once());
    }



    public static void SetupAuthorizedUser(ERoleType? type, MockHttpSession session) {
        session.ClearStorage();
        switch (type) {
            case ERoleType.Superior:

                session.SetObject(LoginController.UserRolesKey, SuperiorRole);
                break;
            case ERoleType.DepartmentManager:
                session.SetObject(LoginController.UserRolesKey, DepartmentManagerRole);
                break;
            case ERoleType.Secretariat:
                session.SetObject(LoginController.UserRolesKey, SecretariatRole);
                break;
            case ERoleType.ProjectManager:
                session.SetObject(LoginController.UserRolesKey, ProjectManagerRoles);
                break;
            case null:
                //no user is logged in 
                session.ClearStorage();
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        session.SetInt32(LoginController.UserIdKey, 1);
        session.SetString(LoginController.UsernameKey, "Username");
        session.SetString(LoginController.UserFullNameKey, "FullName");
        session.SetString(LoginController.UserHasInitPwdKey, "1");
    }

}