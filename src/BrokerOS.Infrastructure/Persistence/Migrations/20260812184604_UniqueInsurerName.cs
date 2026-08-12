using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrokerOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UniqueInsurerName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Insurers_Name",
                table: "Insurers",
                column: "Name",
                unique: true,
                filter: "[OrganizationId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Insurers_OrganizationId_Name",
                table: "Insurers",
                columns: new[] { "OrganizationId", "Name" },
                unique: true,
                filter: "[OrganizationId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Insurers_Name",
                table: "Insurers");

            migrationBuilder.DropIndex(
                name: "IX_Insurers_OrganizationId_Name",
                table: "Insurers");
        }
    }
}
