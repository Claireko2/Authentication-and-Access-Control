namespace MVP_1B2.Models
{
    public class Client : Person
    {
        public decimal Balance { get; set; }
        public DateTime DateOfBirth { get; set; }
        public byte[]? Photo { get; set; }


        // Many-to-many relationship with Service
        public ICollection<ClientService> ClientServices { get; set; }


        // Constructor Initialization
        public Client()
        {
            ClientServices = new HashSet<ClientService>();  // Prevents NullReferenceException
            Balance = 0;  // Default balance
            DateOfBirth = DateTime.MinValue;  // Default DOB (change if needed)
        }

    }

}
