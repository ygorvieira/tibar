using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tibar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InstallmentId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_InstallmentId",
                table: "Transactions",
                column: "InstallmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_InstallmentId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "InstallmentId",
                table: "Transactions");
        }
    }
}
