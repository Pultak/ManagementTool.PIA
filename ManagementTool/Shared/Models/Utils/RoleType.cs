namespace ManagementTool.Shared.Models.Utils;

/// <summary>
///     All possible role types that can be assigned
/// </summary>
public enum RoleType {
    Superior,
    DepartmentManager,
    Secretariat,
    ProjectManager,

    /// <summary>
    ///     Dummy role type to indicate invalid role
    /// </summary>
    NoRole
}