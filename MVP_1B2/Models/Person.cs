namespace MVP_1B2.Models
{
    public class Person
    {
        public Guid ID { get; set; }  // Primary Key
        public string Name { get; set; }
        public string Address { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}

