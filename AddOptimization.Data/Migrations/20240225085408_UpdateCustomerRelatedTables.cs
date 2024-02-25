using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddOptimization.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCustomerRelatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Addresses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Addresses",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
