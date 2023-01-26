using ManagementTool.Server.Models.Business;

namespace ManagementTool.Server.Repository.Users;

public interface IUserRepository {
    public IEnumerable<UserBaseBLL> GetAllUsers();

    /// <summary>
    /// Adds new user to the data source with generated and hashed password and salt 
    /// </summary>
    /// <param name="user">new user model</param>
    /// <param name="pwd">generated hashed password</param>
    /// <param name="salt">salt for password hash</param>
    /// <returns>id of new user, -1 on failure</returns>
    public long AddUser(UserBaseBLL user, string pwd, string salt);
    
    /// <summary>
    /// Deletes the user from data source 
    /// </summary>
    /// <param name="userId">user to delete</param>
    /// <returns>true on success</returns>
    public bool DeleteUser(long userId);

    /// <summary>
    /// Method retrieves user data by his id
    /// </summary>
    /// <param name="id">id of user</param>
    /// <returns></returns>
    public UserBaseBLL? GetUserById(long id);
    /// <summary>
    /// Method gets user data by his name
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public UserBaseBLL? GetUserByName(string username);

    public IEnumerable<UserBaseBLL> GetUsersById(IEnumerable<long> userIds);
    
    /// <summary>
    /// Gets all credentials of user by his username (used during authorization)
    /// </summary>
    /// <param name="username">name of user for credentials</param>
    /// <returns>credentials</returns>
    public (long id, string pwd, string salt)? GetUserCredentials(string username);
    /// <summary>
    /// Gets all credentials of user by his id (used during authorization)
    /// </summary>
    /// <param name="username">name of user for credentials</param>
    /// <returns>credentials</returns>
    public (long id, string pwd, string salt)? GetUserCredentials(long id);

    /// <summary>
    /// Updates the user data 
    /// </summary>
    /// <param name="user"></param>
    /// <returns>true on success</returns>
    public bool UpdateUser(UserBaseBLL user);
    public bool UpdateUserPwd(long userId, string newPwd);

    /// <summary>
    /// Method retrieves all users with specified role
    /// </summary>
    /// <param name="roleId">id of role you want to retrieve</param>
    /// <returns>List of all users with role</returns>
    public IEnumerable<UserBaseBLL> GetAllUsersByRole(long roleId);

    /// <summary>
    /// Method retrieves all users that are superior to specified user
    /// </summary>
    /// <param name="userId">id of user you want superiors of</param>
    /// <returns>list of all ids</returns>
    public IEnumerable<long> GetAllUserSuperiorsIds(long userId);

    /// <summary>
    /// Method retrieves all users and add a flag if they are assigned to it or not
    /// </summary>
    /// <param name="projectId">id of project the user should be assigned to</param>
    /// <returns>List of all users with flags</returns>
    public IEnumerable<DataModelAssignmentBLL<UserBaseBLL>> GetAllUsersAssignedToProjectWrappers(long projectId);

    /// <summary>
    /// Method retrieves all users
    /// </summary>
    /// <param name="projectId">id of project the user should be assigned to</param>
    /// <returns>List of all users with flags</returns>
    public IEnumerable<UserBaseBLL> GetAllUsersAssignedToProject(long projectId);

    /// <summary>
    /// Creates a user/project assignation
    /// </summary>
    /// <param name="usersIds">users that should be part of project</param>
    /// <param name="projectId">project</param>
    /// <returns>true on success</returns>
    public bool AssignUsersToProject(List<long> usersIds, long projectId);
    /// <summary>
    /// Removes all user assignations to the project
    /// </summary>
    /// <param name="usersIds"></param>
    /// <param name="projectId"></param>
    /// <returns>true on success</returns>
    public bool UnassignUsersFromProject(List<long> usersIds, long projectId);

    /// <summary>
    /// Creates a user/superior assignation
    /// </summary>
    /// <param name="superiorsIds"></param>
    /// <param name="superiorId"></param>
    /// <returns>true on success</returns>
    public bool AssignSuperiorsToUser(List<long> superiorsIds, long superiorId);
    /// <summary>
    /// Removes all superior assignations to the project
    /// </summary>
    /// <param name="superiorsIds"></param>
    /// <param name="superiorId"></param>
    /// <returns>true on success</returns>
    public bool UnassignSuperiorsFromUser(List<long> superiorsIds, long userId);
    
    public bool IsUserAssignedToProject(long userId, long projectId);
    
    public UserBaseBLL? GetProjectManagerByRoleId(long roleId);
}