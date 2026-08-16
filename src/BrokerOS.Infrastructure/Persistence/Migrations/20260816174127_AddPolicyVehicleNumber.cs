using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrokerOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyVehicleNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VehicleNumber",
                table: "Policies",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleNumber",
                table: "Policies");
        }
    }
}
