﻿using System.Net;
using System.Security.Claims;
using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Server.Services;
using ManagementTool.Server.Services.Users;
using ManagementTool.ServerTests.MoqModels;
using ManagementTool.Shared.Models.Login;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ManagementTool.ServerTests.Services;

public class AuthServiceTests {
    public const string FullDbUser1Pwd = "TestPWD1";
    public const string FullDbUser1Username = "Username1";
    public const string FullDbUser2Username = "Username2";

    public static RoleBLL[] SuperiorRole = {
        new(1, "Superior", RoleType.Superior)
    };

    public static RoleBLL[] DepartmentManagerRole = {
        new(1, "DepartmentManager", RoleType.DepartmentManager)
    };

    public static RoleBLL[] SecretariatRole = {
        new(1, "Secretariat", RoleType.Secretariat)
    };

    public static RolePL[] SecretariatRolePL = {
        new(1, "Secretariat", RoleType.Secretariat)
    };

    public static RoleBLL[] ProjectManagerRoles = {
        new(1, "TEST1ProjectManager", RoleType.ProjectManager, 1),
        new(2, "TEST2ProjectManager", RoleType.ProjectManager, 2)
    };

    public static string GeneratedUserSalt = "MdAWirg+6/S4U1HDgPBLnQ==";

    // pwd is Abc12345
    public static string GeneratedUserPwd = "G1Ii0Y26NGzjiC1eoMVpuCuPW1xTTlf65c0jq9SJkf0=";
    public readonly string BasicUserPwd = "Abc12345";


    private AuthService _instance;

    private Mock<HttpContext> _mockHttpContext = new();

    private Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
    private MockHttpSession _mockHttpSession = new();

    private Mapper _mockMapper;

    private Mock<IUserRoleRepository> _mockRoleRepository = new();

    private Mock<IUserRepository> _mockUserRepository = new();
    private Mock<IConfiguration> _mockConfiguration = new();



    private static UserBaseBLL _user1 = new() {
        Id = 1,
        EmailAddress = "email@addr.cz",
        FullName = "userrr",
        PrimaryWorkplace = "KIV",
        PwdInit = true,
        Username = "user"
    };

    private ClaimsIdentity identity = new(new[] {
        new Claim(ClaimTypes.Name, FullDbUser1Username),
        new Claim(ClaimTypes.Role, RoleType.Secretariat.ToString()),
        new Claim(IAuthService.UserIdKey, _user1.Id.ToString()),
        new Claim(IAuthService.UsernameKey, _user1.Username),
        new Claim(IAuthService.UserFullNameKey, _user1.FullName),
        new Claim(IAuthService.UserRolesKey, JsonConvert.SerializeObject(SecretariatRole)),
        new Claim(IAuthService.UserHasInitPwdKey, _user1.PwdInit ? "1" : "0"),
    });

    private UserBaseBLL _user2 = new();

    private (long id, string pwd, string salt) _userCredentials1;
    private (long id, string pwd, string salt) _userCredentials2;


    [OneTimeSetUp]
    public void OneTimeSetUp() {
        //init http context
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpSession = new MockHttpSession();

        //create mapper and needed mapper config
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<RoleBLL, RolePL>());
        configuration.AssertConfigurationIsValid();
        _mockMapper = new Mapper(configuration);

        //setup context variables
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);
        _mockHttpContext.Setup(s => s.Session).Returns(_mockHttpSession);

        //setup userService
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRoleRepository = new Mock<IUserRoleRepository>();

        //init config sections
        _mockConfiguration = new Mock<IConfiguration>();
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(x => x["JWTTokenKey"]).Returns("SuperSecretKeyYouCantEvenImagine");
        configSection.Setup(x => x["JWTTimeoutDays"]).Returns("1");
        _mockConfiguration.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configSection.Object);
        

        //init tested instance
        _instance = new AuthService(_mockHttpContextAccessor.Object, _mockUserRepository.Object,
            _mockRoleRepository.Object, _mockMapper, _mockConfiguration.Object);
        
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
        //reset all setups so there are no changes made by tests
        _mockUserRepository.Reset();
        //setup methods and their return values for user repository
        _mockUserRepository.Setup(x => x.GetUserCredentials(1)).Returns(_userCredentials1);
        _mockUserRepository.Setup(x => x.GetUserCredentials(2)).Returns(_userCredentials2);
        _mockUserRepository.Setup(x => x.GetUserCredentials(It.IsAny<string>())).Returns(_userCredentials1);
        _mockUserRepository.Setup(x => x.GetUserByName(FullDbUser1Username)).Returns(_user1);
        _mockUserRepository.Setup(x => x.GetUserByName(FullDbUser2Username)).Returns(_user2);
        _mockUserRepository.Setup(x => x.GetUserById(1)).Returns(_user1);
        _mockUserRepository.Setup(x => x.GetUserById(2)).Returns(_user2);
        _mockUserRepository.Setup(x => x.UpdateUserPwd(It.IsAny<long>(), It.IsAny<string>())).Returns(true);

        //setup methods and their return values for role repository
        _mockRoleRepository.Reset();
        _mockRoleRepository.Setup(x => x.GetUserRolesByUserId(1)).Returns(SecretariatRole);
        _mockRoleRepository.Setup(x => x.GetUserRolesByUserId(2)).Returns(SecretariatRole);

        //clear session so there is no data left in next test
        _mockHttpSession.ClearStorage();

        //setup identity of user passed by JWT
        _mockHttpContext.Setup(x => x.User).Returns(
            new ClaimsPrincipal(identity));

    }


    [Test]
    [TestCase("", "")]
    [TestCase("user", "")]
    public void Login_EmptyCredentials_BadRequest(string username, string pwd) {
        //test login method when no credentials are passed
        AuthRequest authPayload = new(username, pwd);

        var (authResponse, statusCode) = _instance.Login(authPayload);

        Assert.Multiple(() => {
            Assert.That(authResponse, Is.EqualTo(AuthResponse.BadRequest));
            Assert.That(statusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    


    [Test]
    public void Login_WrongUsernamePassed_UnknownUser() {
        //test login method when user with passed name does not exist
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
        //test login method when passed password is incorrect
        AuthRequest authPayload = new("Username1", "pwd");

        var (authResponse, statusCode) = _instance.Login(authPayload);

        Assert.Multiple(() => {
            Assert.That(authResponse, Is.EqualTo(AuthResponse.WrongPassword));
            Assert.That(statusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }


    [Test]
    public void Login_ValidCredentials_OkLoggedIn() {
        //test login method everything went ok
        AuthRequest authPayload = new(FullDbUser2Username, BasicUserPwd);

        var (authResponse, statusCode) = _instance.Login(authPayload);

        
        Assert.Multiple(() => {
            Assert.That(authResponse, Is.EqualTo(AuthResponse.Success));
            Assert.That(statusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }
    
    [Test]
    public void Logout_LoggedIn_Success() {
        //test logout method and be successful
        var response = _instance.Logout();

        Assert.Multiple(() => {
            Assert.That(response, Is.EqualTo(AuthResponse.Success));
            Assert.That(_mockHttpSession.GetInt32(IAuthService.UserIdKey), Is.Null);
        });
    }


    [Test]
    public void GetLoggedInUser_LoggedIn_Success() {
        //test GetLoggedInUser and result is successfully filled
        var response = _instance.GetLoggedInUser();

        if (response == null) {
            Assert.Fail();
        }


        Assert.Multiple(() => {
            Assert.That(response!.UserName, Is.EqualTo(_user1.Username));
            Assert.That(response.FullName, Is.EqualTo(_user1.FullName));
            Assert.That(response.Roles, Has.Length.EqualTo(SecretariatRolePL.Length));
            Assert.That(response.Roles![0].Name, Is.EqualTo(SecretariatRole[0].Name));
            Assert.That(response.Roles![0].Id, Is.EqualTo(SecretariatRole[0].Id));
            Assert.That(response.HasInitPwd, Is.True);
        });
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
        //test all possible invalid passwords
        var response = _instance.LoggedInUserChangePwd(pwd);

        Assert.That(response, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }


    [Test]
    public void LoggedInUserChangePwd_LoggedInValidPwd_OK() {
        //test valid password
        var response = _instance.LoggedInUserChangePwd(BasicUserPwd);

        Assert.That(response, Is.EqualTo(HttpStatusCode.OK));
        _mockUserRepository.Verify(x => x.UpdateUserPwd(It.Is<long>(
                xId => xId == 1),
            It.Is<string>(xPwd => xPwd.Equals(GeneratedUserPwd))
        ), Times.Once());
    }


    /*public static void SetupAuthorizedUser(RoleType? type, MockHttpSession session) {
        session.ClearStorage();
        switch (type) {
            case RoleType.Superior:

                session.SetObject(IAuthService.UserRolesKey, SuperiorRole);
                break;
            case RoleType.DepartmentManager:
                session.SetObject(IAuthService.UserRolesKey, DepartmentManagerRole);
                break;
            case RoleType.Secretariat:
                session.SetObject(IAuthService.UserRolesKey, SecretariatRole);
                break;
            case RoleType.ProjectManager:
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
    }*/
    
}