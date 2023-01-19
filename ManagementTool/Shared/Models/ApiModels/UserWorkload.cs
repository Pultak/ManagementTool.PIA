namespace ManagementTool.Shared.Models.ApiModels; 

public class UserWorkload {

    public string UserFullName { get; set; }

    public double[] AllWorkload { get; set; }
    public double[] ActiveWorkload { get; set; }

    public UserWorkload() {
        AllWorkload = new double[] {};
        ActiveWorkload = new double[] {};
        UserFullName = string.Empty;
    }

    
    public UserWorkload(string userFullName, int arraySize) {
        AllWorkload = new double[arraySize];
        ActiveWorkload = new double[arraySize];
        UserFullName = userFullName;
    }


    public UserWorkload(string userFullName, double[] allWorkload, double[] activeWorkload) {
        UserFullName = userFullName;
        AllWorkload = allWorkload;
        ActiveWorkload = activeWorkload;
    }
}