using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AddOptimization.Data.Migrations
{
    /// <inheritdoc />
    public partial class Licensing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "License",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LicenseKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LicenseDuration = table.Column<int>(type: "int", nullable: false),
                    NoOfDevices = table.Column<int>(type: "int", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_License", x => x.Id);
                    table.ForeignKey(
                        name: "FK_License_ApplicationUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_License_ApplicationUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_License_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LicenseDevice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MachineName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicenseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenseDevice_ApplicationUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LicenseDevice_ApplicationUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LicenseDevice_License_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "License",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_License_CreatedByUserId",
                table: "License",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_License_CustomerId",
                table: "License",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_License_UpdatedByUserId",
                table: "License",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseDevice_CreatedByUserId",
                table: "LicenseDevice",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseDevice_LicenseId",
                table: "LicenseDevice",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseDevice_UpdatedByUserId",
                table: "LicenseDevice",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LicenseDevice");

            migrationBuilder.DropTable(
                name: "License");
        }
    }
}
