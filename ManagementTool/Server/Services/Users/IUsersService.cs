using ManagementTool.Shared.Models.Presentation;
using ManagementTool.Shared.Models.Utils;

namespace ManagementTool.Server.Services.Users;

/// <summary>
/// Service to manage all user related methods
/// </summary>
public interface IUsersService {
    /// <summary>
    /// Gets all users from data source
    /// </summary>
    /// <returns>All users from data source</returns>
    public IEnumerable<UserBasePL> GetUsers();

    /// <summary>
    /// Returns all users from project that are assigned to project
    /// </summary>
    /// <param name="projectId">id of the project for desired users</param>
    /// <returns>null if invalid project, project users otherwise</returns>
    public IEnumerable<DataModelAssignmentPL<UserBasePL>>? GetAllUsersUnderProject(long projectId);

    /// <summary>
    /// Method for creation of new user. It also contains validation and hashing of pwd
    /// </summary>
    /// <param name="user">new user you want to create</param>
    /// <param name="pwd">password for new user</param>
    /// <returns>enum of all possible creation states + new user id on success</returns>
    public (UserCreationResponse response, long newId) CreateUser(UserBasePL user, string pwd);

    /// <summary>
    /// Method for updating of existing user. It checks his presence in data source and validates the new data
    /// </summary>
    /// <param name="dbUser">User you want to update</param>
    /// <returns>enum of all possible creation states + new user id on success</returns>
    public UserCreationResponse UpdateUser(UserBasePL dbUser);

    /// <summary>
    /// Method for deletion of user. The user must be present in data source
    /// </summary>
    /// <param name="userId">Id of user you want to delete</param>
    /// <returns>true on success, false otherwise</returns>
    public bool DeleteUser(long userId);

    /// <summary>
    /// Updates all user superiors in the data sources.
    /// Then list of superior is split and compared by existing superior assignments in the data source
    /// </summary>
    /// <param name="newAssignedSuperiors">list of all assigned superiors</param>
    /// <param name="user">user you want to update superiors of</param>
    public void UpdateUserSuperiorAssignments(IEnumerable<UserBasePL> newAssignedSuperiors, UserBasePL user);

    /// <summary>
    /// Method for getting all users with desired role
    /// </summary>
    /// <param name="roleType">type of the user you want to get</param>
    /// <returns>user list of all users with correct role</returns>
    public IEnumerable<UserBasePL> GetAllUsersWithRole(RoleType roleType);


    /// <summary>
    /// Method for receiving all users superiors ids. 
    /// </summary>
    /// <param name="userId">user of which you want to get superiors from</param>
    /// <returns>all superior ids from desired user</returns>
    public IEnumerable<long>? GetAllUserSuperiorsIds(long userId);
}