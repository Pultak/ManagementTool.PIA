using ManagementTool.Shared.Models.Api.Payloads;
using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Users; 

public interface IUsersService {

    public IEnumerable<UserBase> GetUsers();

    IEnumerable<DataModelAssignment<UserBase>>? GetAllUsersUnderProject(long projectId);

    EUserCreationResponse CreateUser(User user);

    public EUserCreationResponse UpdateUser(UserBase dbUser);

    public bool DeleteUser(long userId);

    public void UpdateUserSuperiorAssignments(IReadOnlyCollection<UserBase> newAssignedSuperiors, UserBase user);

    public IEnumerable<UserBase> GetAllUsersWithRole(ERoleType roleType);

    public IEnumerable<long>? GetAllUserSuperiorsIds(long userId);

}