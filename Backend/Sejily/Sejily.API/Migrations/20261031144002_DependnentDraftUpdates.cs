using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sejily.API.Migrations
{
    /// <inheritdoc />
    public partial class DependnentDraftUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "KinshipVerifiedAt",
                table: "DependentRegistrationDrafts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Relationship",
                table: "DependentRegistrationDrafts",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KinshipVerifiedAt",
                table: "DependentRegistrationDrafts");

            migrationBuilder.DropColumn(
                name: "Relationship",
                table: "DependentRegistrationDrafts");
        }
    }
}
