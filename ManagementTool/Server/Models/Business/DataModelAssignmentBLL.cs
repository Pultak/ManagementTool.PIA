namespace ManagementTool.Server.Models.Business;

/// <summary>
/// DataModelAssignment is used to indicate if the model is already assigned to some property.
/// For example role assignment => checks if it already assigned to the user or not
/// </summary>
/// <typeparam name="T">Model type you need your assignment of </typeparam>
public class DataModelAssignmentBLL<T> {

    /// <summary>
    /// Flag if the model is already assigned to the desired group
    /// </summary>
    public bool IsAssigned { get; set; }

    /// <summary>
    /// Model you want to check assignment of 
    /// </summary>
    public T DataModel { get; set; }

    /*public DataModelAssignmentBLL() {

    }*/
    public DataModelAssignmentBLL(T dataModel) {
        DataModel = dataModel;
    }

    public DataModelAssignmentBLL(bool assigned, T model) {
        IsAssigned = assigned;
        DataModel = model;
    }
}