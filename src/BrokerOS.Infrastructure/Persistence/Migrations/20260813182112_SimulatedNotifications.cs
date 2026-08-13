using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrokerOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimulatedNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<long>(type: "bigint", nullable: false),
                    RenewalId = table.Column<long>(type: "bigint", nullable: false),
                    ClientId = table.Column<long>(type: "bigint", nullable: true),
                    RecipientType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ReminderMilestoneDays = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Renewals_RenewalId",
                        column: x => x.RenewalId,
                        principalTable: "Renewals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ClientId",
                table: "Notifications",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_OrganizationId_CreatedAtUtc",
                table: "Notifications",
                columns: new[] { "OrganizationId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_OrganizationId_RenewalId",
                table: "Notifications",
                columns: new[] { "OrganizationId", "RenewalId" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_PublicId",
                table: "Notifications",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RenewalId_ReminderMilestoneDays",
                table: "Notifications",
                columns: new[] { "RenewalId", "ReminderMilestoneDays" },
                unique: true,
                filter: "[ReminderMilestoneDays] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
