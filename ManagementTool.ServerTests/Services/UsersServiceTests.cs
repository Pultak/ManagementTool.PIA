using System.Security.Claims;
using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Repository.Projects;
using ManagementTool.Server.Repository.Users;
using ManagementTool.Server.Services.Roles;
using ManagementTool.Server.Services.Users;
using ManagementTool.ServerTests.MoqModels;
using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Presentation.Api.Requests;
using ManagementTool.Shared.Models.Utils;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ManagementTool.ServerTests.Services;

public class UsersServiceTests {
    public const int DummyUserCount = 3;


    private readonly ProjectBLL _dummyProject1 = new() {
        Id = 1,
        ProjectName = "DummyID1"
    };

    private readonly RolePL _dummyRole = new(1, "pMan", RoleType.ProjectManager, 1);

    private readonly List<RoleBLL> _dummyRoles = new(new[] {
        new RoleBLL(1, "pMan", RoleType.ProjectManager, 1),
        new RoleBLL(2, "pMan2", RoleType.ProjectManager, 2)
    });


    private readonly UserBaseBLL _dummyUser1 = new() {
        Id = 1,
        Username = "DummyID1"
    };

    private readonly List<DataModelAssignmentBLL<UserBaseBLL>> _dummyUserAssignment = new();
    private readonly List<UserBaseBLL> _dummyUsers = new();

    private readonly UserUpdateRequest _dummyUserUpdateRequest = new(new UserBasePL());

    private UsersService _instance;
    private Mock<IAuthService> _mockAuthService = new();

    private Mock<HttpContext> _mockHttpContext = new();
    private MockHttpSession _mockHttpSession = new();
    private Mapper _mockMapper;

    private Mock<IProjectRepository> _mockProjectRepository = new();
    private Mock<IRolesService> _mockRoleService = new();

    private Mock<IUserRepository> _mockUserRepository = new();


    [OneTimeSetUp]
    public void OneTimeSetup() {
        //init 
        for (var i = 0; i < DummyUserCount; i++) {
            var user = new UserBaseBLL {
                Id = i + 1,
                EmailAddress = $"test{i + 1}@address.cz",
                FullName = $"FullName{i + 1}",
                PrimaryWorkplace = $"Workplace{i + 1}",
                Username = $"Username{i + 1}",
                PwdInit = true
            };
            _dummyUsers.Add(user);

            _dummyUserAssignment.Add(new DataModelAssignmentBLL<UserBaseBLL>(i % 2 == 0, user));
        }


        var configuration = new MapperConfiguration(cfg => {
            cfg.CreateMap<RoleBLL, RolePL>();
            cfg.CreateMap<RolePL, RoleBLL>();
            cfg.CreateMap<UserBaseBLL, UserBasePL>();
            cfg.CreateMap<UserBasePL, UserBaseBLL>();
        });

        configuration.AssertConfigurationIsValid();

        _mockMapper = new Mapper(configuration);

        _mockHttpContext = new Mock<HttpContext>();

        _mockHttpSession = new MockHttpSession();

        _mockHttpContext.Setup(s => s.Session).Returns(_mockHttpSession);

        //setup userService
        _mockUserRepository = new Mock<IUserRepository>();
        _mockAuthService = new Mock<IAuthService>();
        _mockProjectRepository = new Mock<IProjectRepository>();
        _mockRoleService = new Mock<IRolesService>();

        _instance = new UsersService(_mockUserRepository.Object, _mockAuthService.Object, _mockRoleService.Object,
            _mockMapper);
    }

    [SetUp]
    public void Setup() {
        _mockUserRepository.Reset();
        _mockUserRepository.Setup(x => x.GetAllUsers()).Returns(_dummyUsers);
        _mockUserRepository.Setup(x => x.GetAllUsersByRole(It.IsAny<long>())).Returns(_dummyUsers);
        _mockUserRepository.Setup(x => x.GetAllUsersAssignedToProjectWrappers(
            It.IsAny<long>())).Returns(_dummyUserAssignment);
        _mockUserRepository.Setup(x => x.GetUserByName(_dummyUser1.Username)).Returns(_dummyUser1);

        _mockUserRepository.Setup(x => x.AddUser(
            It.IsAny<UserBaseBLL>(), It.IsAny<string>(), It.IsAny<string>())).Returns(1);
        _mockUserRepository.Setup(x => x.UpdateUser(It.IsAny<UserBaseBLL>())).Returns(true);

        _mockUserRepository.Setup(x => x.GetUserById(10)).Returns((UserBaseBLL?)null);
        _mockUserRepository.Setup(x => x.GetUserById(1)).Returns(_dummyUser1);
        _mockUserRepository.Setup(x => x.DeleteUser(It.IsAny<long>())).Returns(true);
        _mockUserRepository.Setup(x => x.GetAllUserSuperiorsIds(It.IsAny<long>()))
            .Returns(new long[] { 2, 3 });
        _mockUserRepository.Setup(x => x.GetUserByName(It.Is<string>(
            xUsername => xUsername.Equals(_dummyUser1.Username)))).Returns(_dummyUser1);


        _mockRoleService.Reset();
        _mockRoleService.Setup(x => x.GetRoleByType(It.IsAny<RoleType>())).Returns(_dummyRole);
        /*_mockRoleService.Setup(x => x.GetUserRolesByUserId(1)).Returns(_dummyRoles);
        _mockRoleService.Setup(x => x.AssignRolesToUser(
            It.IsAny<List<Role>>(), 1)).Returns(true);
        _mockRoleService.Setup(x => x.UnassignRolesFromUser(
            It.IsAny<List<Role>>(), 1)).Returns(true);
        */
        _mockAuthService.Reset();
        _mockAuthService.Setup(x => x.GenerateSalt())
            .Returns(Convert.FromBase64String(AuthServiceTests.GeneratedUserSalt));
        _mockAuthService.Setup(x => x.HashPwd(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(AuthServiceTests.GeneratedUserPwd);

        _mockProjectRepository.Reset();
        _mockProjectRepository.Setup(x => x.GetProjectById(10)).Returns((ProjectBLL?)null);
        _mockProjectRepository.Setup(x => x.GetProjectById(1)).Returns(_dummyProject1);


        _mockHttpContext.Setup(x => x.User).Returns(
            new ClaimsPrincipal(
                new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, _dummyUser1.Username),
                    new Claim(ClaimTypes.Role, RoleType.Secretariat.ToString()),
                    new Claim(IAuthService.UserIdKey, _dummyUser1.Id.ToString()),
                    new Claim(IAuthService.UsernameKey, _dummyUser1.Username),
                    new Claim(IAuthService.UserFullNameKey, _dummyUser1.FullName),
                    new Claim(IAuthService.UserRolesKey, JsonConvert.SerializeObject(_dummyRole)),
                    new Claim(IAuthService.UserHasInitPwdKey, _dummyUser1.PwdInit ? "1" : "0"),
                })));

    }

    [Test]
    public void GetUsers_Empty_Users() {
        var users = _instance.GetAllUsersUnderProject(1);

        Assert.Multiple(() => { Assert.That(users, Is.Not.Null); });
    }


    [Test]
    [TestCase("")]
    [TestCase("//aw455")]
    [TestCase("ba")]
    [TestCase("velkyPredator115ú")]
    [TestCase("wwwr 89")]
    [TestCase("123456789012345678901234567890dlouhejmeno")]
    public void CreateUser_InvalidUsernameName_UnprocessableEntity(string username) {

        var dummyPayload = GenerateValidUserCreationRequest();
        dummyPayload.UpdatedUser.Username = username;

        var creationResponse = _instance.CreateUser(dummyPayload.UpdatedUser, dummyPayload.Pwd);
        Assert.That(creationResponse.response, Is.EqualTo(UserCreationResponse.InvalidUsername));
        Assert.That(creationResponse.newId, Is.EqualTo(0));
    }

    [Test]
    [TestCase("")]
    [TestCase("ba")]
    [TestCase(
        "MocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmeno")]
    public void CreateUser_InvalidFullName_UnprocessableEntity(string fullname) {
        var dummyPayload = GenerateValidUserCreationRequest();
        dummyPayload.UpdatedUser.FullName = fullname;

        var creationResponse = _instance.CreateUser(dummyPayload.UpdatedUser, dummyPayload.Pwd);
        Assert.That(creationResponse.response, Is.EqualTo(UserCreationResponse.InvalidFullName));
        Assert.That(creationResponse.newId, Is.EqualTo(0));
    }


    [Test]
    [TestCase("")]
    [TestCase(
        "MocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmenoMocdlouhejmeno")]
    public void CreateUser_InvalidWorkplace_UnprocessableEntity(string workplace) {
        var dummyPayload = GenerateValidUserCreationRequest();
        dummyPayload.UpdatedUser.PrimaryWorkplace = workplace;

        var creationResponse = _instance.CreateUser(dummyPayload.UpdatedUser, dummyPayload.Pwd);
        Assert.That(creationResponse.response, Is.EqualTo(UserCreationResponse.InvalidWorkplace));
        Assert.That(creationResponse.newId, Is.EqualTo(0));
    }


    [Test]
    [TestCase("emailus@maximus")]
    [TestCase("")]
    [TestCase("Vrazdicí.kralicek@wajfaoi.aaaaaa")]
    [TestCase("Vpohodě@mail.yo")]
    public void CreateUser_InvalidEmailAddress_UnprocessableEntity(string emailAddress) {
        var dummyPayload = GenerateValidUserCreationRequest();
        dummyPayload.UpdatedUser.EmailAddress = emailAddress;

        var creationResponse = _instance.CreateUser(dummyPayload.UpdatedUser, dummyPayload.Pwd);
        Assert.That(creationResponse.response, Is.EqualTo(UserCreationResponse.InvalidEmail));
        Assert.That(creationResponse.newId, Is.EqualTo(0));
    }

    [Test]
    public void CreateUser_UsedUsername_UsernameTaken() {
        var dummyPayload = GenerateValidUserCreationRequest();
        dummyPayload.UpdatedUser.Username = _dummyUser1.Username;

        var creationResponse = _instance.CreateUser(dummyPayload.UpdatedUser, dummyPayload.Pwd);
        Assert.That(creationResponse.response, Is.EqualTo(UserCreationResponse.UsernameTaken));
        Assert.That(creationResponse.newId, Is.EqualTo(0));
    }


    [Test]
    [TestCase("Heslo123")]
    [TestCase("Hch84W69MAWLiid")]
    [TestCase("WHgaoh86231afwa2")]
    [TestCase("Hal6971awws33")]
    public void CreateUser_ValidData_Ok(string pwd) {
        var dummyPayload = GenerateValidUserCreationRequest();
        dummyPayload.Pwd = pwd;


        var creationResponse = _instance.CreateUser(dummyPayload.UpdatedUser, dummyPayload.Pwd);

        Assert.That(creationResponse.response, Is.EqualTo(UserCreationResponse.Ok));
        Assert.That(creationResponse.newId, Is.EqualTo(1));

        _mockUserRepository.Verify(x => x.AddUser(It.IsAny<UserBaseBLL>(),
            AuthServiceTests.GeneratedUserPwd, AuthServiceTests.GeneratedUserSalt), Times.Once);
    }
    /*todo remove
    [Test]
    public void CreateUser_4RolesAssignment_2RolesUpdated() {
        AuthServiceTests.SetupAuthorizedUser(ERoleType.Secretariat, _mockHttpSession);

        var dummyPayload = GenerateValidUserCreationRequest();

        dummyPayload.AssignedRoles = new List<DataModelAssignmentPL<RolePL>>(new[] {
            new DataModelAssignmentPL<RolePL>(true, new RolePL(1, "pman", ERoleType.ProjectManager, 1)),
            new DataModelAssignmentPL<RolePL>(false, new RolePL(2, "pman2", ERoleType.ProjectManager, 2)),
            new DataModelAssignmentPL<RolePL>(true, new RolePL(3, "secret", ERoleType.Secretariat, null)),
            new DataModelAssignmentPL<RolePL>(false, new RolePL(4, "sup", ERoleType.Superior, null))
        });
        var creationResponse = _instance.CreateUser(dummyPayload);

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
    */


    [Test]
    public void UpdateUser_InvalidUserData_BadRequest() {
        var updateResponse = _instance.UpdateUser(_dummyUserUpdateRequest.UpdatedUser);

        Assert.That(updateResponse, Is.EqualTo(UserCreationResponse.InvalidUsername));
        //other validation options already tested in creation
    }

    [Test]
    public void UpdateUser_ValidData_Ok() {
        var payload = GenerateValidUserUpdateRequest();
        var updateResponse = _instance.UpdateUser(payload.UpdatedUser);

        Assert.That(updateResponse, Is.EqualTo(UserCreationResponse.Ok));
        _mockUserRepository.Verify(x => x.UpdateUser(It.IsAny<UserBaseBLL>()), Times.Once());
    }


    [Test]
    public void DeleteUser_NegativeUserId_BadRequest() {
        var deletionOk = _instance.DeleteUser(-1);

        Assert.That(deletionOk, Is.False);
    }


    [Test]
    public void DeleteUser_IdNotAssigned_NotFound() {
        var deletionOk = _instance.DeleteUser(10);

        Assert.That(deletionOk, Is.False);
    }


    [Test]
    public void DeleteUser_ValidData_Ok() {
        var deletionOk = _instance.DeleteUser(1);

        Assert.That(deletionOk, Is.True);
        _mockUserRepository.Verify(x => x.GetUserById(1), Times.Once());
        _mockUserRepository.Verify(x => x.DeleteUser(It.Is<long>(xId =>
            xId == _dummyUser1.Id)), Times.Once());
    }

    [Test]
    public void GetAllUsersUnderProject_NegativeUserId_Null() {
        var ok = _instance.GetAllUsersUnderProject(-1);

        Assert.That(ok, Is.Null);
    }


    [Test]
    public void GetAllUsersUnderProject_ValidData_Ok() {
        var ok = _instance.GetAllUsersUnderProject(1);

        _mockUserRepository.Verify(x => x.GetAllUsersAssignedToProject(1), Times.Once());
    }

    [Test]
    public void GetAllUsersWithRole_BadRole_Null() {
        var users = _instance.GetAllUsersWithRole(RoleType.NoRole);

        Assert.That(users, Is.Empty);
        _mockRoleService.Verify(x => x.GetRoleByType(RoleType.Secretariat), Times.Never);
    }


    [Test]
    public void GetAllUsersWithRole_NonExistentRole_EmptyArray() {
        _mockRoleService.Setup(x => x.GetRoleByType(RoleType.Secretariat)).Returns((RolePL?)null);
        var users = _instance.GetAllUsersWithRole(RoleType.Secretariat);

        Assert.That(users, Is.Empty);
        _mockRoleService.Verify(x => x.GetRoleByType(RoleType.Secretariat), Times.Once);
    }

    [Test]
    public void GetAllUsersWithRole_ValidData_Ok() {
        var users = _instance.GetAllUsersWithRole(RoleType.Secretariat);

        Assert.That(users, Is.Not.Empty);
        _mockRoleService.Verify(x => x.GetRoleByType(RoleType.Secretariat), Times.Once);
        _mockUserRepository.Verify(x => x.GetAllUsersByRole(1), Times.Once());
    }

    [Test]
    public void GetAllUserSuperiorsIds_NegativeId_Null() {
        var users = _instance.GetAllUserSuperiorsIds(-1);

        Assert.That(users, Is.Null);
        _mockUserRepository.Verify(x => x.GetAllUserSuperiorsIds(It.IsAny<long>()), Times.Never);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void GetAllUserSuperiorsIds_ValidData_Ok(long id) {
        var users = _instance.GetAllUserSuperiorsIds(id);

        Assert.That(users, Is.Not.Empty);
        _mockUserRepository.Verify(x => x.GetAllUserSuperiorsIds(id), Times.Once());
    }

    [Test]
    public void UpdateUserSuperiorAssignments_2Superiors_1NewlyAssigned1Unassigned() {
        var supArray = new UserBasePL[] {
            new() { Id = 1 },
            new() { Id = 2 }
        };

        var user = new UserBasePL {
            Id = 1
        };

        _instance.UpdateUserSuperiorAssignments(supArray, user);

        _mockUserRepository.Verify(x => x.AssignSuperiorsToUser(It.Is<List<long>>(
            xList => xList.Count == 1), 1), Times.Once());

        _mockUserRepository.Verify(x => x.UnassignSuperiorsFromUser(It.Is<List<long>>(
            xList => xList.Count == 1), 1), Times.Once());
    }


    private UserUpdateRequest GenerateValidUserUpdateRequest() {
        var user = new UserBasePL(1, "username", "fullName",
            "workplace", "email@Address.cz", true);

        UserUpdateRequest dummyPayload = new(user);
        return dummyPayload;
    }

    private UserCreationRequest GenerateValidUserCreationRequest() {
        var user = new UserBasePL(1, "username", "fullName",
            "workplace", "email@Address.cz", true);


        UserCreationRequest dummyPayload = new(user, Array.Empty<DataModelAssignmentPL<RolePL>>().ToList(),
            Array.Empty<UserBasePL>().ToList(), "Abc12345");
        return dummyPayload;
    }
}