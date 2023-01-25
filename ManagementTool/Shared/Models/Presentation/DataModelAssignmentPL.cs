namespace ManagementTool.Shared.Models.Presentation;

/// <summary>
///     DataModelAssignment is used to indicate if the model is already assigned to some property.
///     For example role assignment => checks if it already assigned to the user or not
/// </summary>
/// <typeparam name="T">Model type you need your assignment of </typeparam>
public class DataModelAssignmentPL<T> where T : new(){

    public DataModelAssignmentPL() {
        DataModel = new T();
    }
    public DataModelAssignmentPL(T dataModel) => DataModel = dataModel;

    public DataModelAssignmentPL(bool assigned, T model) {
        IsAssigned = assigned;
        DataModel = model;
    }


    /// <summary>
    ///     Flag if the model is already assigned to the desired group
    /// </summary>
    public bool IsAssigned { get; set; }

    /// <summary>
    ///     Model you want to check assignment of
    /// </summary>
    public T DataModel { get; set; }
}