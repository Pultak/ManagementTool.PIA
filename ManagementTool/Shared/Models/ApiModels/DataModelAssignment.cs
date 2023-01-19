namespace ManagementTool.Shared.Models.ApiModels; 

public class DataModelAssignment<T> {
    
    public bool IsAssigned { get; set; }
    public T DataModel { get; set; }

    public DataModelAssignment() {

    }
    public DataModelAssignment(T dataModel) {
        this.DataModel = dataModel;
    }

    public DataModelAssignment(bool assigned, T model) {
        IsAssigned = assigned;
        DataModel = model;
    }
}