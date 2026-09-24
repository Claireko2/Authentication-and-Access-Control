using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVP_1B2.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Services_ServiceID",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_ServiceID",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ServiceID",
                table: "Clients");

            migrationBuilder.AlterColumn<decimal>(
                name: "Rate",
                table: "Services",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Rate",
                table: "Services",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceID",
                table: "Clients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ServiceID",
                table: "Clients",
                column: "ServiceID");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Services_ServiceID",
                table: "Clients",
                column: "ServiceID",
                principalTable: "Services",
                principalColumn: "ID");
        }
    }
}
