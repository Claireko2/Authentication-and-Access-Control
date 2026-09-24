using Microsoft.AspNetCore.Identity;

namespace MVP_1B2.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Guid? ClientID { get; set; }
        public Guid? EmployeeID { get; set; }

        public Client? Client { get; set; }
        public Employee? Employee { get; set; }
    }
}