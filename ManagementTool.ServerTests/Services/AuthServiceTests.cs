using ManagementTool.Shared.Models.Utils;
using System.Net;
using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Server.Services;
using ManagementTool.Server.Services.Users;
using ManagementTool.ServerTests.MoqModels;
using ManagementTool.Shared.Models.Login;
using Microsoft.AspNetCore.Http;
using ManagementTool.Shared.Models.Presentation;

namespace ManagementTool.ServerTests.Services; 

public class AuthServiceTests {



    public static RoleBLL[] SuperiorRole = {
        new(1, "Superior", ERoleType.Superior )
    };

    public static RoleBLL[] DepartmentManagerRole = {
        new(1, "DepartmentManager", ERoleType.DepartmentManager)
    };

    public static RoleBLL[] SecretariatRole = {
        new(1, "Secretariat", ERoleType.Secretariat)
    };
    public static RolePL[] SecretariatRolePL = {
        new(1, "Secretariat", ERoleType.Secretariat)
    };

    public static RoleBLL[] ProjectManagerRoles = {
        new(1, "TEST1ProjectManager", ERoleType.ProjectManager, 1),
        new(2, "TEST2ProjectManager", ERoleType.ProjectManager, 2)
    };


    public const string FullDbUser1Pwd = "TestPWD1";
    public const string FullDbUser1Username = "Username1";
    public const string FullDbUser2Username = "Username2";

    private (long id, string pwd, string salt) _userCredentials1;
    private (long id, string pwd, string salt) _userCredentials2;

    public static string GeneratedUserSalt = "MdAWirg+6/S4U1HDgPBLnQ==";
    // pwd is Abc12345
    public static string GeneratedUserPwd = "G1Ii0Y26NGzjiC1eoMVpuCuPW1xTTlf65c0jq9SJkf0=";
    public readonly string BasicUserPwd = "Abc12345";

    private UserBaseBLL _user1 = new();
    private UserBaseBLL _user2 = new();


    private AuthService _instance;

    private Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();

    private Mock<HttpContext> _mockHttpContext = new();
    private MockHttpSession _mockHttpSession = new();
    
    private Mock<IUserRoleRepository> _mockRoleRepository = new();

    private Mock<IUserRepository> _mockUserRepository = new();

    private Mapper _mockMapper;

    
    [OneTimeSetUp]
    public void OneTimeSetUp() {


        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpSession = new MockHttpSession();

        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<RoleBLL, RolePL>());

        configuration.AssertConfigurationIsValid();

        _mockMapper = new Mapper(configuration);

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);
        _mockHttpContext.Setup(s => s.Session).Returns(_mockHttpSession);

        //setup userService
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRoleRepository = new Mock<IUserRoleRepository>();

        _instance = new AuthService(_mockHttpContextAccessor.Object, _mockUserRepository.Object,
            _mockRoleRepository.Object, _mockMapper);
        
        _user1 = new UserBaseBLL{
            Id = 1,
            EmailAddress = "test1@addr.cz",
            FullName = "FullName1",
            PrimaryWorkplace = $"Workplace1",
            Username = FullDbUser1Username,
            PwdInit = true
        };
        _user2 = new UserBaseBLL {
            Id = 2,
            EmailAddress = "test2@addr.cz",
            FullName = "FullName2",
            PrimaryWorkplace = "Workplace2",
            Username = FullDbUser2Username,
            PwdInit = true
        };
        _userCredentials1 = (1, GeneratedUserPwd, GeneratedUserSalt);
        _userCredentials2 = (2, GeneratedUserPwd, GeneratedUserSalt);

    }

    
    [SetUp]
    public void Setup() {
        _mockUserRepository.Reset();
        _mockUserRepository.Setup(x => x.GetUserCredentials(1)).Returns(_userCredentials1);
        _mockUserRepository.Setup(x => x.GetUserCredentials(2)).Returns(_userCredentials2);
        _mockUserRepository.Setup(x => x.GetUserCredentials(It.IsAny<string>())).Returns(_userCredentials1);
        _mockUserRepository.Setup(x => x.GetUserByName(FullDbUser1Username)).Returns(_user1);
        _mockUserRepository.Setup(x => x.GetUserByName(FullDbUser2Username)).Returns(_user2);
        _mockUserRepository.Setup(x => x.GetUserById(1)).Returns(_user1);
        _mockUserRepository.Setup(x => x.GetUserById(2)).Returns(_user2);
        _mockUserRepository.Setup(x => x.UpdateUserPwd(It.IsAny<long>(), It.IsAny<string>())).Returns(true);
        
        _mockRoleRepository.Reset();
        _mockRoleRepository.Setup(x => x.GetUserRolesByUserId(1)).Returns(SecretariatRole);
        _mockRoleRepository.Setup(x => x.GetUserRolesByUserId(2)).Returns(SecretariatRole);

        _mockHttpSession.ClearStorage();
    }



    [Test]
    [TestCase("", "")]
    [TestCase("user", "")]
    public void Login_EmptyCredentials_BadRequest(string username, string pwd) {

        AuthRequest authPayload = new(username, pwd);

        var (authResponse, statusCode) = _instance.Login(authPayload);

        Assert.Multiple(() => {
            Assert.That(authResponse, Is.EqualTo(AuthResponse.BadRequest));
            Assert.That(statusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    
    [Test]
    public void Login_ValidData_AlreadyLoggedIn() {
        SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        AuthRequest authPayload = new("username", "pwd");

        var (authResponse, statusCode) = _instance.Login(authPayload);
        Assert.Multiple(() => {
            Assert.That(authResponse, Is.EqualTo(AuthResponse.AlreadyLoggedIn));
            Assert.That(statusCode, Is.EqualTo(HttpStatusCode.OK));
        });
        
    }

    
    
    [Test]
    public void Login_WrongUsernamePassed_UnknownUser() {
        _mockUserRepository.Setup(x => x.GetUserCredentials(It.IsAny<string>()))
            .Returns(((long id, string pwd, string salt)?)null);
        AuthRequest authPayload = new("pepa", "pwd");

        var (authResponse, statusCode) = _instance.Login(authPayload);

        Assert.Multiple(() => {
            Assert.That(authResponse, Is.EqualTo(AuthResponse.UnknownUser));
            Assert.That(statusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }


    [Test]
    public void Login_WrongPwdPassed_WrongPassword() {

        AuthRequest authPayload = new("Username1", "pwd");

        var (authResponse, statusCode) = _instance.Login(authPayload);

        Assert.Multiple(() => {
            Assert.That(authResponse, Is.EqualTo(AuthResponse.WrongPassword));
            Assert.That(statusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    
    [Test]
    public void Login_ValidCredentials_OkLoggedIn() {

        AuthRequest authPayload = new(FullDbUser2Username, BasicUserPwd);

        var (authResponse, statusCode) = _instance.Login(authPayload);


        var roles = _mockHttpSession.GetObject<RoleBLL[]>(IAuthService.UserRolesKey);

        //Assert.Multiple(() => {
            Assert.That(authResponse, Is.EqualTo(AuthResponse.Success));
            Assert.That(statusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(_mockHttpSession.GetInt32(IAuthService.UserIdKey), Is.EqualTo(_user1.Id));
            Assert.That(_mockHttpSession.GetString(IAuthService.UsernameKey), Is.EqualTo(_user1.Username));
            Assert.That(_mockHttpSession.GetString(IAuthService.UserFullNameKey), Is.EqualTo(_user1.FullName));

            Assert.That(roles, Has.Length.EqualTo(SecretariatRolePL.Length));
            Assert.That(roles![0].Name, Is.EqualTo(SecretariatRole[0].Name));
            Assert.That(roles[0].Id, Is.EqualTo(SecretariatRole[0].Id));
            Assert.That(_mockHttpSession.GetInt32(IAuthService.UserHasInitPwdKey) != 0, Is.EqualTo(_user1.PwdInit));
        //});


    }

    
    [Test]
    public void Logout_NotLoggedIn_UnknownUser() {
        var response = _instance.Logout();

        Assert.That(response, Is.EqualTo(AuthResponse.UnknownUser));
    }
    
    [Test]
    public void Logout_LoggedIn_Success() {
        SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        var response = _instance.Logout();

        Assert.Multiple(() => {
            Assert.That(response, Is.EqualTo(AuthResponse.Success));
            Assert.That(_mockHttpSession.GetInt32(IAuthService.UserIdKey), Is.Null);
        });

    }

    
    
    [Test]
    public void GetLoggedInUser_LoggedIn_Success() {
        SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        var response = _instance.GetLoggedInUser();

        if (response == null) {
            Assert.Fail();
        }

        
        //Assert.Multiple(() => {
            Assert.That(response!.UserName, Is.EqualTo("Username"));
            Assert.That(response.FullName, Is.EqualTo("FullName"));
            Assert.That(response.Roles, Has.Length.EqualTo(SecretariatRolePL.Length));
            Assert.That(response.Roles![0].Name, Is.EqualTo(SecretariatRole[0].Name));
            Assert.That(response.Roles![0].Id, Is.EqualTo(SecretariatRole[0].Id));
            Assert.That(response.HasInitPwd, Is.True);
        //});

    }

    
    
    [Test]
    public void LoggedInUserChangePwd_NotLoggedIn_Unauthorized() {
        var response = _instance.LoggedInUserChangePwd("");

        Assert.That(response, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    
    
    [Test]
    public void LoggedInUserChangePwd_LoggedInUnknownUser_NotFound() {
        SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        //set id for non existent user
        _mockHttpSession.SetInt32(IAuthService.UserIdKey, 10);

        var response = _instance.LoggedInUserChangePwd("");

        Assert.That(response, Is.EqualTo(HttpStatusCode.NotFound));
    }

    
    [Test]
    [TestCase("emailus@maximus")]
    [TestCase("")]
    [TestCase("Abc123")]
    [TestCase("HodneTezkeHeslo:-)")]
    [TestCase("12345678")]
    [TestCase("123456aa")]
    [TestCase("123456BB")]
    [TestCase("BBAAaaBBcc")]
    [TestCase("Mocdlouheheslomocdlouheheslomocdlouheheslomocdlouheheslomocdlouheheslo1")]
    public void LoggedInUserChangePwd_InvalidPwd_UnprocessableEntity(string pwd) {
        SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        
        var response = _instance.LoggedInUserChangePwd(pwd);

        Assert.That(response, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }
    
    
    [Test]
    public void LoggedInUserChangePwd_LoggedInValidPwd_OK() {
        SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        
        var response = _instance.LoggedInUserChangePwd(BasicUserPwd);

        Assert.That(response, Is.EqualTo(HttpStatusCode.OK));
        _mockUserRepository.Verify(x => x.UpdateUserPwd(It.Is<long>(
            xId => xId == 1), 
            It.Is<string>(xPwd => xPwd.Equals(GeneratedUserPwd))
            ), Times.Once());
    }


    

    public static void SetupAuthorizedUser(ERoleType? type, MockHttpSession session) {
        session.ClearStorage();
        switch (type) {
            case ERoleType.Superior:

                session.SetObject(IAuthService.UserRolesKey, SuperiorRole);
                break;
            case ERoleType.DepartmentManager:
                session.SetObject(IAuthService.UserRolesKey, DepartmentManagerRole);
                break;
            case ERoleType.Secretariat:
                session.SetObject(IAuthService.UserRolesKey, SecretariatRole);
                break;
            case ERoleType.ProjectManager:
                session.SetObject(IAuthService.UserRolesKey, ProjectManagerRoles);
                break;
            case null:
                //no user is logged in 
                session.ClearStorage();
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        session.SetInt32(IAuthService.UserIdKey, 1);
        session.SetString(IAuthService.UsernameKey, "Username");
        session.SetString(IAuthService.UserFullNameKey, "FullName");
        session.SetString(IAuthService.UserHasInitPwdKey, "1");
    }

}