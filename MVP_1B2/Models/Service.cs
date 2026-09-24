using System.ComponentModel.DataAnnotations;

namespace MVP_1B2.Models
{

    

    public class Service
    {
        public Service()
        {
            ClientServices = new HashSet<ClientService>();
            Employees = new HashSet<Employee>();
        }


        public Guid ID { get; set; }  // Primary Key

        [Required]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "Only characters are allowed.")]
        [StringLength(10, ErrorMessage = "Service name cannot be longer than 10 characters.")]
        public string Name { get; set; }


        [Range(0, double.MaxValue, ErrorMessage = "Rate must be non-negative.")]
        public decimal Rate { get; set; }

        // Many-to-many relationship with Client
        public ICollection<ClientService> ClientServices { get; set; }
      
        // One-to-many relationship with Employee
        public ICollection<Employee> Employees { get; set; }
    }

}
