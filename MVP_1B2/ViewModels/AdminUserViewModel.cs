namespace MVP_1B2.ViewModels
{
    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public Guid? ClientID { get; set; }

        public Guid? EmployeeID { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();
    }
}
