using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MVP_1B2.Models;

namespace MVP_1B2
{
    public class ClientContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Person> Persons { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ClientService> ClientServices { get; set; }

        public ClientContext(DbContextOptions<ClientContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>().ToTable("People");
            modelBuilder.Entity<Client>().ToTable("Clients");
            modelBuilder.Entity<Employee>().ToTable("Employees");
            modelBuilder.Entity<Service>().ToTable("Services");
            modelBuilder.Entity<ClientService>().ToTable("ClientServices");


            
            // Configure the many-to-many relationship between Client and Service
            modelBuilder.Entity<ClientService>()
                .HasKey(cs => new { cs.ClientID, cs.ServiceID });
            modelBuilder.Entity<ClientService>()
                .HasOne(cs => cs.Client)
                .WithMany(c => c.ClientServices)
                .HasForeignKey(cs => cs.ClientID);
            modelBuilder.Entity<ClientService>()
                .HasOne(cs => cs.Service)
                .WithMany(s => s.ClientServices)
                .HasForeignKey(cs => cs.ServiceID);

            // Configure Employee-Service one-to-many relationship
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Service)
                .WithMany(s => s.Employees)
                .HasForeignKey(e => e.ServiceID)
                .IsRequired(false);

            // Add constraints to Service
            modelBuilder.Entity<Service>()
                .Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(10);

            modelBuilder.Entity<Service>()
                .Property(s => s.Rate)
                .HasDefaultValue(0)
                .IsRequired();

            // ApplicationUser → Client
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Client)
                .WithMany()
                .HasForeignKey(u => u.ClientID)
                .OnDelete(DeleteBehavior.NoAction);

            // ApplicationUser → Employee
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Employee)
                .WithMany()
                .HasForeignKey(u => u.EmployeeID)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
