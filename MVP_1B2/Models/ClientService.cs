namespace MVP_1B2.Models
{
    public class ClientService
    {
        public Guid ClientID { get; set; }
        public Client Client { get; set; }

        public Guid ServiceID { get; set; }
        public Service Service { get; set; }
    }

}
