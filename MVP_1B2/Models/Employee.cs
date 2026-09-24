namespace MVP_1B2.Models
{   


    public class Employee : Person
    {
        public Employee()
        {
            Service = null;  // Explicitly setting to null for clarity
        }
        public decimal Salary { get; set; }

        // Many-to-one relationship with Service
        public Guid? ServiceID { get; set; }
        public Service? Service { get; set; }

    }

}
