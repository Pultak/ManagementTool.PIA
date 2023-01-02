namespace ManagementTool.Shared.Models.Database
{
    public class UserRoleXRefs
    {

        public long IdUser { get; set; }
        public long IdRole { get; set; }

        public DateTime AssignedDate { get; set; }
    }
}
