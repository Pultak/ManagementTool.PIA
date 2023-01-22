namespace ManagementTool.Shared.Models.Presentation.Api.Payloads;
/// <summary>
/// User and his active + all assignment workload.  
/// </summary>
public class UserWorkload
{

    /// <summary>
    /// Full name of the user of which assignments are these workloads created of
    /// </summary>
    public string UserFullName { get; set; }

    /// <summary>
    /// Workloads of assignment of any state
    /// </summary>
    public double[] AllWorkload { get; set; }
    /// <summary>
    /// Workload of only active assignments
    /// </summary>
    public double[] ActiveWorkload { get; set; }

    /// <summary>
    /// Creates the user workload with empty arrays
    /// </summary>
    public UserWorkload()
    {
        AllWorkload = Array.Empty<double>();
        ActiveWorkload = Array.Empty<double>();
        UserFullName = string.Empty;
    }

    /// <summary>
    /// Init the userWorkload with workload arrays of desired arraySize
    /// </summary>
    /// <param name="userFullName">Name of the user of which the workload is for</param>
    /// <param name="arraySize">size of the workload arrays</param>
    public UserWorkload(string userFullName, int arraySize)
    {
        AllWorkload = new double[arraySize];
        ActiveWorkload = new double[arraySize];
        UserFullName = userFullName;
    }


    public UserWorkload(string userFullName, double[] allWorkload, double[] activeWorkload)
    {
        UserFullName = userFullName;
        AllWorkload = allWorkload;
        ActiveWorkload = activeWorkload;
    }
}