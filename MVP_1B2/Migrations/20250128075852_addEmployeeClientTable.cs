using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVP_1B2.Migrations
{
    /// <inheritdoc />
    public partial class addEmployeeClientTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the Clients table
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Address = table.Column<string>(nullable: false),
                    Balance = table.Column<decimal>(nullable: false),
                    DateOfBirth = table.Column<DateTime>(nullable: false),
                    Photo = table.Column<byte[]>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.ID);
                });

            // Create the Employees table
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Address = table.Column<string>(nullable: false),
                    Salary = table.Column<decimal>(nullable: false),
                    // Add the foreign key column for ServiceID (Nullable if not every employee has a service)
                    ServiceID = table.Column<Guid>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.ID);
                    // Add a foreign key constraint to link the ServiceID with Services table
                    table.ForeignKey(
                        name: "FK_Employees_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services", // Assuming Services table exists
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict); // or Cascade if deleting a service should delete employees
                });

            
            
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the ClientServices table (many-to-many relationship)
            migrationBuilder.DropTable(
                name: "ClientServices");

            // Drop the Employees table
            migrationBuilder.DropTable(
                name: "Employees");

            // Drop the Services table
            migrationBuilder.DropTable(
                name: "Services");

            // Drop the Clients table
            migrationBuilder.DropTable(
                name: "Clients");
        }

    }
}
