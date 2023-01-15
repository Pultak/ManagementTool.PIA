using System.Net;
using ManagementTool.Server.Controllers;
using ManagementTool.Server.Services.Projects;
using ManagementTool.Server.Services.Users;
using ManagementTool.Server.Services;
using ManagementTool.ServerTests.MoqModels;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Http;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.ApiModels;
using ManagementTool.Shared.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;

namespace ManagementTool.ServerTests.Controllers;

public class UsersControllerTests {
    private UsersController _controller;

    private Mock<HttpContext> _mockHttpContext;
    private HttpResponse _mockHttpResponse;
    private MockHttpSession _mockHttpSession;
    
    private Mock<IProjectDataService> _mockProjectService;
    private Mock<IUserRoleDataService> _mockRoleService;

    private Mock<IUserDataService> _mockUserService;

    public const int DummyUserCount = 3;
    private readonly List<User> _dummyUsers = new();
    private readonly List<DataModelAssignment<UserBase>> _dummyUserAssignment = new();

    private readonly UserUpdatePayload<User> _dummyUserPayload = new (new User());
    private readonly UserUpdatePayload<UserBase> _dummyUserBasePayload = new (new UserBase());
    
    private readonly List<Role> _dummyRoles = new(new[] {
        new Role(1, "pman", ERoleType.ProjectManager, 1),
        new Role(2, "pman2", ERoleType.ProjectManager, 2),
    });


    private readonly User _dummyUser1 = new(){
        Id = 1,
        Username = "DummyID1"
    };


    private readonly Project _dummyProject1 = new(){
        Id = 1,
        ProjectName = "DummyID1"
    };


    [OneTimeSetUp]
    public void OneTimeSetup() {
        //init 
        for (var i = 0; i < DummyUserCount; i++) {
            var salt = UsersController.GenerateSalt();
            var user = new User{
                Id = i + 1,
                EmailAddress = $"test{i + 1}@addr.cz",
                FullName = $"FullName{i + 1}",
                PrimaryWorkplace = $"Workplace{i + 1}",
                Pwd = LoginController.HashPwd($"TestPWD{i + 1}", salt),
                Username = $"Username{i + 1}",
                PwdInit = true,
                Salt = Convert.ToBase64String(salt)
            };
            _dummyUsers.Add(user);

            _dummyUserAssignment.Add(new DataModelAssignment<UserBase>(i % 2 == 0, user));
        }


        _mockHttpContext = new Mock<HttpContext>();

        _mockHttpResponse = new MoqHttpResponse();
        _mockHttpSession = new MockHttpSession();

        _mockHttpContext.Setup(s => s.Response).Returns(_mockHttpResponse);
        _mockHttpContext.Setup(s => s.Session).Returns(_mockHttpSession);
        
        //setup userService
        _mockUserService = new Mock<IUserDataService>();

        _mockProjectService = new Mock<IProjectDataService>();
        _mockRoleService = new Mock<IUserRoleDataService>();

        _controller = new UsersController(_mockUserService.Object, _mockProjectService.Object, _mockRoleService.Object) {
            ControllerContext = {
                HttpContext = _mockHttpContext.Object
            }
        };
    }

    [SetUp]
    public void Setup() {

        _mockUserService.Reset();
        _mockUserService.Setup(x => x.GetAllUsers()).Returns(_dummyUsers);
        _mockUserService.Setup(x => x.GetAllUsersAssignedToProject(It.IsAny<long>())).Returns(_dummyUserAssignment);

        _mockUserService.Setup(x => x.AddUser(It.IsAny<User>())).Returns(1);
        _mockUserService.Setup(x => x.UpdateUser(It.IsAny<User>())).Returns(true);

        _mockUserService.Setup(x => x.GetUserById(10)).Returns((User?)null);
        _mockUserService.Setup(x => x.GetUserById(1)).Returns(_dummyUser1);
        _mockUserService.Setup(x => x.DeleteUser(It.IsAny<User>())).Returns(true);
        _mockUserService.Setup(x => x.AssignUserToProject(It.IsAny<User>(), It.IsAny<Project>())).Returns(true);


        _mockRoleService.Reset();
        _mockRoleService.Setup(x => x.GetUserRolesByUserId(1)).Returns(_dummyRoles);
        _mockRoleService.Setup(x => x.AssignRolesToUser(
            It.IsAny<List<Role>>(), 1)).Returns(true);
        _mockRoleService.Setup(x => x.UnassignRolesFromUser(
            It.IsAny<List<Role>>(), 1)).Returns(true);


        _mockProjectService.Reset();
        _mockProjectService.Setup(x => x.GetProjectById(10)).Returns((Project?)null);
        _mockProjectService.Setup(x => x.GetProjectById(1)).Returns(_dummyProject1);
    }

    [Test]
    [TestCase(null)]
    [TestCase(ERoleType.Superior)]
    [TestCase(ERoleType.ProjectManager)]
    [TestCase(ERoleType.DepartmentManager)]
    public void GetAllUsers_PossibleUsers_Unauthorized(ERoleType? userType) {
        LoginControllerTests.SetupAuthorizedUser(userType, _mockHttpSession);
        var users = _controller.GetAllUsers();
        Assert.IsNull(users);
    }

    
    [Test]
    public void GetAllUsers_Secretariat_Users() {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        var users = _controller.GetAllUsers();
        Assert.IsNotNull(_dummyUsers);
        Assert.That(users, Is.EqualTo(_dummyUsers));
    }
    
    [Test]
    [TestCase(null)]
    [TestCase(ERoleType.Superior)]
    public void GetAllUsersForProject_UnauthorizedUsers_Unauthorized(ERoleType? userType) {
        LoginControllerTests.SetupAuthorizedUser(userType, _mockHttpSession);
        var users = _controller.GetAllUsersForProject(1);
        Assert.IsNull(users);
        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public void GetAllUsersForProject_NegativeId_BadRequest() {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        var users = _controller.GetAllUsersForProject(-1);
        Assert.IsNull(users);
        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
    }

    
    [Test]
    public void GetAllUsersForProject_Secretariat_Users() {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        var users = _controller.GetAllUsersForProject(1);
        Assert.IsNotNull(users);
        Assert.That(users, Is.EqualTo(_dummyUserAssignment));
    }


    
    [Test]
    [TestCase(null)]
    [TestCase(ERoleType.Superior)]
    [TestCase(ERoleType.ProjectManager)]
    [TestCase(ERoleType.DepartmentManager)]
    public void CreateUser_UnauthorizedUsers_Unauthorized(ERoleType? userType) {
        LoginControllerTests.SetupAuthorizedUser(userType, _mockHttpSession);
        var userId = _controller.CreateUser(_dummyUserPayload);
        Assert.That(userId, Is.EqualTo(-1));
        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.Unauthorized));
    }
    
    [Test]
    [TestCase("")]
    [TestCase("//aw455")]
    [TestCase("ba")]
    [TestCase("velkyPredator115ú")]
    [TestCase("wwwr 89")]
    [TestCase("123456789012345678901234567890dlouhejmeno")]
    public void CreateUser_InvalidPayloadInvalidUsernameName_UnprocessableEntity(string username) {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        var dummyPayload = GenerateValidUserUpdatePayload<User>();
        dummyPayload.UpdatedUser.Username = username;

        var userId = _controller.CreateUser(dummyPayload);
        Assert.That( userId, Is.EqualTo(-1));
        Assert.That( _controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.UnprocessableEntity));
    }
     
    [Test]
    [TestCase("")]
    [TestCase("ba")]
    [TestCase("MocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmeno")]
    public void CreateUser_InvalidPayloadInvalidFullName_UnprocessableEntity(string fullname) {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        var dummyPayload = GenerateValidUserUpdatePayload<User>();
        dummyPayload.UpdatedUser.FullName = fullname;

        var userId = _controller.CreateUser(dummyPayload);
        Assert.That( userId, Is.EqualTo(-1));
        Assert.That( _controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.UnprocessableEntity));
    }
    
    
    [Test]
    [TestCase("")]
    [TestCase("ba")]
    [TestCase("MocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmeno")]
    public void CreateUser_InvalidPayloadInvalidWorkplace_UnprocessableEntity(string workplace) {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        var dummyPayload = GenerateValidUserUpdatePayload<User>();
        dummyPayload.UpdatedUser.FullName = workplace;

        var userId = _controller.CreateUser(dummyPayload);
        Assert.That( userId, Is.EqualTo(-1));
        Assert.That( _controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.UnprocessableEntity));
    }

    
    
    [Test]
    [TestCase("emailus@maximus")]
    [TestCase("")]
    [TestCase("Vrazdicí.kralicek@wajfaoi.aaaaaa")]
    [TestCase("Vpohodì@mail.yo")]
    public void CreateUser_InvalidPayloadInvalidEmailAddress_UnprocessableEntity(string emailAddress) {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        var dummyPayload = GenerateValidUserUpdatePayload<User>();
        dummyPayload.UpdatedUser.EmailAddress = emailAddress;

        var userId = _controller.CreateUser(dummyPayload);
        Assert.That( userId, Is.EqualTo(-1));
        Assert.That( _controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.UnprocessableEntity));
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
    public void CreateUser_InvalidPayloadInvalidPassword_UnprocessableEntity(string pwd) {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        var dummyPayload = GenerateValidUserUpdatePayload<User>();
        dummyPayload.UpdatedUser.Pwd = pwd;

        var match = UserUtils.PasswordRegex.IsMatch(pwd);


        var userId = _controller.CreateUser(dummyPayload);
        Assert.That( userId, Is.EqualTo(-1));
        Assert.That( _controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.UnprocessableEntity));
    }



    [Test]
    [TestCase("Heslo123")]
    [TestCase("Hch84W69MAWLiid")]
    [TestCase("WHgaoh86231afwa2")]
    [TestCase("Hal6971awws33")]
    public void CreateUser_ValidData_Ok(string pwd) {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        var dummyPayload = GenerateValidUserUpdatePayload<User>();
        dummyPayload.UpdatedUser.Pwd = pwd;


        var userId = _controller.CreateUser(dummyPayload);

        Assert.That(userId, Is.EqualTo(1));
        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        Assert.That(dummyPayload.UpdatedUser.Pwd, Is.EqualTo(LoginController.HashPwd(pwd,
            Convert.FromBase64String(dummyPayload.UpdatedUser.Salt))));
    }

    [Test]
    public void CreateUser_4RolesAssignment_2RolesUpdated() {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        var dummyPayload = GenerateValidUserUpdatePayload<User>();

        dummyPayload.AssignedRoles = new List<DataModelAssignment<Role>>(new[] {
            new DataModelAssignment<Role>(true, new Role(1, "pman", ERoleType.ProjectManager, 1)),
            new DataModelAssignment<Role>(false, new Role(2, "pman2", ERoleType.ProjectManager, 2)),
            new DataModelAssignment<Role>(true, new Role(3, "secret", ERoleType.Secretariat, null)),
            new DataModelAssignment<Role>(false, new Role(4, "sup", ERoleType.Superior, null))
        });

        var userId = _controller.CreateUser(dummyPayload);

        Assert.That(userId, Is.EqualTo(1));

        //called?
        _mockRoleService.Verify(x => x.AssignRolesToUser(It.IsAny<List<Role>>(), 1), Times.Once());
        //called with correct param?
        _mockRoleService.Verify(x => x.AssignRolesToUser(It.Is<List<Role>>(x => 
            x.Count == 1 && x[0].Name.Equals("secret")), 1), Times.Once());

        //called?
        _mockRoleService.Verify(x => x.UnassignRolesFromUser(It.IsAny<List<Role>>(), 1), Times.Once());
        //called with correct param?
        _mockRoleService.Verify(x => x.UnassignRolesFromUser(It.Is<List<Role>>(ux =>
            ux.Count == 1 && ux[0].Name.Equals("pman2")), 1), Times.Once());
    }





    [Test]
    [TestCase(null)]
    [TestCase(ERoleType.Superior)]
    [TestCase(ERoleType.ProjectManager)]
    [TestCase(ERoleType.DepartmentManager)]
    public void UpdateUser_UnauthorizedUsers_Unauthorized(ERoleType? userType) {
        LoginControllerTests.SetupAuthorizedUser(userType, _mockHttpSession);
        
        _controller.UpdateUser(_dummyUserBasePayload);
        
        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.Unauthorized));
    }


    [Test]
    public void UpdateUser_InvalidUserData_BadRequest() {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        
        _controller.UpdateUser(_dummyUserBasePayload);
        
        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
    }

    [Test]
    public void UpdateUser_ValidData_Ok() {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        var payload = GenerateValidUserUpdatePayload<UserBase>();
        _controller.UpdateUser(payload);
        
        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        _mockUserService.Verify(x => x.UpdateUser(It.IsAny<User>()), Times.Once());
    }


    [Test]
    [TestCase(null)]
    [TestCase(ERoleType.Superior)]
    [TestCase(ERoleType.ProjectManager)]
    [TestCase(ERoleType.DepartmentManager)]
    public void DeleteUser_UnauthorizedUsers_Unauthorized(ERoleType? userType) {
        LoginControllerTests.SetupAuthorizedUser(userType, _mockHttpSession);

        _controller.DeleteUser(_dummyUserBasePayload.UpdatedUser.Id);

        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.Unauthorized));
    }


    [Test]
    public void DeleteUser_NegativeUserId_BadRequest() {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        _controller.DeleteUser(-1);

        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
    }

    
    [Test]
    public void DeleteUser_IdNotAssigned_NotFound() {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        
        _controller.DeleteUser(10);

        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
    }

    
    
    [Test]
    public void DeleteUser_ValidData_Ok() {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);
        
        _controller.DeleteUser(1);

        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        _mockUserService.Verify(x => x.GetUserById(1), Times.Once());
        _mockUserService.Verify(x => x.DeleteUser(It.Is<User>(x => 
            x.Id == _dummyUser1.Id && x.Username.Equals(_dummyUser1.Username))), Times.Once());

    }

    
    [Test]
    [TestCase(null)]
    [TestCase(ERoleType.Superior)]
    [TestCase(ERoleType.ProjectManager)]
    [TestCase(ERoleType.DepartmentManager)]
    public void AssignUserToProject_UnauthorizedUsers_Unauthorized(ERoleType? userType) {
        LoginControllerTests.SetupAuthorizedUser(userType, _mockHttpSession);

        _controller.AssignUserToProject(1, 1);

        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.Unauthorized));
    }

    [Test]
    [TestCase(1, -1)]
    [TestCase(-1, 1)]
    public void AssignUserToProject_InvalidIds_BadRequest(long idUser, long idProject) {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        _controller.AssignUserToProject(idUser, idProject);

        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
    }
    
    [Test]
    [TestCase(10, 1)]
    [TestCase(1, 10)]
    public void AssignUserToProject_IdNotAssigned_NotFound(long idUser, long idProject) {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        _controller.AssignUserToProject(idUser, idProject);

        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));

        _mockUserService.Verify(x => x.GetUserById(idUser), Times.Once());
        _mockProjectService.Verify(x => x.GetProjectById(idProject), idProject == 10 ? Times.Once() : Times.Never());
    }
    
    [Test]
    public void AssignUserToProject_ValidUserAndProject_OK() {
        LoginControllerTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        _controller.AssignUserToProject(1, 1);

        Assert.That(_controller.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));

        _mockUserService.Verify(x => x.GetUserById(1), Times.Once());
        _mockProjectService.Verify(x => x.GetProjectById(1), Times.Once());

        _mockUserService.Verify(x => x.AssignUserToProject(
            It.Is<User>(user => user.Id == _dummyUser1.Id), 
            It.Is<Project>(project => project.Id == _dummyProject1.Id)), Times.Once());
    }



    private UserUpdatePayload<T> GenerateValidUserUpdatePayload<T>() where T: UserBase {
        var user = (new User(1, "username", "fullName",
            "workplace", "email@Address.cz", "SuperSecretPwd1", "TBD", true) as T)!;
        
        UserUpdatePayload<T> dummyPayload = new(user!);
        return dummyPayload;
    }

}