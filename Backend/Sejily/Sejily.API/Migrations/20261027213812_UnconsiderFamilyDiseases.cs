using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sejily.API.Migrations
{
    /// <inheritdoc />
    public partial class UnconsiderFamilyDiseases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalEncounters_Workplaces_WorkplaceId",
                table: "MedicalEncounters");

            migrationBuilder.DropIndex(
                name: "IX_Medications_ActiveIngredient",
                table: "Medications");

            migrationBuilder.DropIndex(
                name: "IX_MedicalEncounters_WorkplaceId",
                table: "MedicalEncounters");

            migrationBuilder.DropColumn(
                name: "ActiveIngredient",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "WorkplaceId",
                table: "MedicalEncounters");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalEncounters_WorkplacesId",
                table: "MedicalEncounters",
                column: "WorkplacesId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalEncounters_Workplaces_WorkplacesId",
                table: "MedicalEncounters",
                column: "WorkplacesId",
                principalTable: "Workplaces",
                principalColumn: "WorkplaceId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalEncounters_Workplaces_WorkplacesId",
                table: "MedicalEncounters");

            migrationBuilder.DropIndex(
                name: "IX_MedicalEncounters_WorkplacesId",
                table: "MedicalEncounters");

            migrationBuilder.AddColumn<string>(
                name: "ActiveIngredient",
                table: "Medications",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkplaceId",
                table: "MedicalEncounters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Medications_ActiveIngredient",
                table: "Medications",
                column: "ActiveIngredient");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalEncounters_WorkplaceId",
                table: "MedicalEncounters",
                column: "WorkplaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalEncounters_Workplaces_WorkplaceId",
                table: "MedicalEncounters",
                column: "WorkplaceId",
                principalTable: "Workplaces",
                principalColumn: "WorkplaceId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
