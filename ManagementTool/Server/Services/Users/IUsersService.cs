using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Users; 

public interface IUsersService {

    public IEnumerable<UserBasePL> GetUsers();

    public IEnumerable<DataModelAssignmentPL<UserBasePL>>? GetAllUsersUnderProject(long projectId);

    public EUserCreationResponse CreateUser(UserBasePL user, string pwd);

    public EUserCreationResponse UpdateUser(UserBasePL dbUser);

    public bool DeleteUser(long userId);

    public void UpdateUserSuperiorAssignments(IEnumerable<UserBasePL> newAssignedSuperiors, UserBasePL user);

    public IEnumerable<UserBasePL> GetAllUsersWithRole(ERoleType roleType);

    public IEnumerable<long>? GetAllUserSuperiorsIds(long userId);

}