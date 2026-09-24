using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVP_1B2.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "People",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "People");
        }
    }
}
