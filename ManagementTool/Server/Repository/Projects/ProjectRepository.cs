using AutoMapper;
using ManagementTool.Server.Models.Business;
using ManagementTool.Server.Models.Database;

namespace ManagementTool.Server.Repository.Projects;

public class ProjectRepository : IProjectRepository {
    private readonly ManToolDbContext _db;

    public ProjectRepository(ManToolDbContext db, IMapper mapper) {
        _db = db;
        Mapper = mapper;
    }

    private IMapper Mapper { get; }

    public IEnumerable<ProjectBLL> GetAllProjects() {
        var result = _db.Project?.ToList();
        return result == null ? Enumerable.Empty<ProjectBLL>() : Mapper.Map<ProjectBLL[]>(result);
    }

    /// <summary>
    /// Method retrieves one project by his name
    /// </summary>
    /// <param name="name">name of the project</param>
    /// <returns>null if not present</returns>
    public ProjectBLL? GetProjectByName(string name) {
        var result = _db.Project?.SingleOrDefault(project => string.Equals(project.ProjectName, name));
        return result == null ? null : Mapper.Map<ProjectBLL>(result);
    }

    /// <summary>
    /// Method retrieves one project by his id
    /// </summary>
    /// <param name="projectId">id of the project</param>
    /// <returns>null if not present</returns>
    public ProjectBLL? GetProjectById(long projectId) {
        var result = _db.Project?.Find(projectId);
        return result == null ? null : Mapper.Map<ProjectBLL>(result);
    }

    public IEnumerable<ProjectBLL> GetProjectsByIds(IEnumerable<long> projectIds) {
        var result = _db.Project?.Where(x => projectIds.Contains(x.Id));
        return result == null ? Enumerable.Empty<ProjectBLL>() : Mapper.Map<IEnumerable<ProjectBLL>>(result);
    }


    public long AddProject(ProjectBLL project) {
        var dbProject = Mapper.Map<ProjectDAL>(project);
        dbProject.Id = default;
        _db.Project?.Add(dbProject);
        var rowsChanged = _db.SaveChanges();
        if (rowsChanged <= 0) {
            return -1;
        }

        // new id is part of the db object now
        return dbProject.Id;
    }

    public bool DeleteProject(long id) {
        var dbProject = _db.Project?.Find(id);
        if (dbProject == null) {
            return false;
        }

        _db.Project?.Remove(dbProject);
        var rowsChanged = _db.SaveChanges();

        return rowsChanged > 0;
    }

    public bool UpdateProject(ProjectBLL project) {
        var dbProject = Mapper.Map<ProjectDAL>(project);
        _db.Project?.Update(dbProject);
        var rowsChanged = _db.SaveChanges();

        return rowsChanged > 0;
    }


    /// <summary>
    /// Checks if all users are under any of passed projects
    /// </summary>
    /// <param name="usersIds"></param>
    /// <param name="projectIds"></param>
    /// <returns></returns>
    public bool AreUsersUnderProjects(long[] usersIds, long[] projectIds) {
        var count = _db.UserProjectXRefs?.Where(x => usersIds.Contains(x.IdUser) && projectIds.Contains(x.IdProject))
            .DistinctBy(x => x.IdUser).Count();
        return count != null && count == usersIds.Length;
    }


    public bool DeleteProjectUserAssignments(long projectId) {
        var userAssignments = _db.UserProjectXRefs?.Where(o => o.IdProject == projectId);
        if (userAssignments == null || !userAssignments.Any()) {
            //project can be without assignments
            return true;
        }

        _db.Remove(userAssignments);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }

    /* method could be used upon deletion of projects
     public bool DeleteAllProjectAssignments(long projectId) {
        var assignments = _db.Assignment?.Where(o => o.ProjectId == projectId);
        if (assignments == null || !assignments.Any()) {
            //project can be without assignments
            return true;
        }

        _db.Remove(assignments);
        var changedRows = _db.SaveChanges();
        return changedRows > 0;
    }*/
}