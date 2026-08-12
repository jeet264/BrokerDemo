using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrokerOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PolicyTermRollover : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "NextPolicyId",
                table: "Policies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PreviousPolicyId",
                table: "Policies",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Policies_NextPolicyId",
                table: "Policies",
                column: "NextPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_PreviousPolicyId",
                table: "Policies",
                column: "PreviousPolicyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Policies_NextPolicyId",
                table: "Policies",
                column: "NextPolicyId",
                principalTable: "Policies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Policies_PreviousPolicyId",
                table: "Policies",
                column: "PreviousPolicyId",
                principalTable: "Policies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Policies_NextPolicyId",
                table: "Policies");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Policies_PreviousPolicyId",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Policies_NextPolicyId",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Policies_PreviousPolicyId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "NextPolicyId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "PreviousPolicyId",
                table: "Policies");
        }
    }
}
