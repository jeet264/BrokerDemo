using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrokerOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenewalReminderMilestones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_RenewalId",
                table: "Tasks");

            migrationBuilder.AddColumn<int>(
                name: "ReminderMilestoneDays",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_RenewalId_ReminderMilestoneDays",
                table: "Tasks",
                columns: new[] { "RenewalId", "ReminderMilestoneDays" },
                unique: true,
                filter: "[RenewalId] IS NOT NULL AND [ReminderMilestoneDays] IS NOT NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_RenewalId_ReminderMilestoneDays",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ReminderMilestoneDays",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_RenewalId",
                table: "Tasks",
                column: "RenewalId");
        }
    }
}
