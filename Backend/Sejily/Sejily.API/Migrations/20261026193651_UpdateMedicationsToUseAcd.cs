using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sejily.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMedicationsToUseAcd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicationUsage_MedicalEncounters_MedicationId",
                table: "MedicationUsage");

            migrationBuilder.AlterColumn<int>(
                name: "DiseaseStatus",
                table: "PatientDisease",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsFamilyDisease",
                table: "PatientDisease",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AtcCode",
                table: "Medications",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AtcVersion",
                table: "Medications",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WorkplaceId",
                table: "MedicalEncounters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PatientDisease_PatientId_DiseaseId_IsFamilyDisease",
                table: "PatientDisease",
                columns: new[] { "PatientId", "DiseaseId", "IsFamilyDisease" },
                unique: true,
                filter: "[MedicalEncounterId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationUsage_MedicalEncounterId",
                table: "MedicationUsage",
                column: "MedicalEncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_Medications_AtcCode_AtcVersion",
                table: "Medications",
                columns: new[] { "AtcCode", "AtcVersion" },
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationUsage_MedicalEncounters_MedicalEncounterId",
                table: "MedicationUsage",
                column: "MedicalEncounterId",
                principalTable: "MedicalEncounters",
                principalColumn: "MedicalEncounterId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalEncounters_Workplaces_WorkplaceId",
                table: "MedicalEncounters");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicationUsage_MedicalEncounters_MedicalEncounterId",
                table: "MedicationUsage");

            migrationBuilder.DropIndex(
                name: "IX_PatientDisease_PatientId_DiseaseId_IsFamilyDisease",
                table: "PatientDisease");

            migrationBuilder.DropIndex(
                name: "IX_MedicationUsage_MedicalEncounterId",
                table: "MedicationUsage");

            migrationBuilder.DropIndex(
                name: "IX_Medications_AtcCode_AtcVersion",
                table: "Medications");

            migrationBuilder.DropIndex(
                name: "IX_MedicalEncounters_WorkplaceId",
                table: "MedicalEncounters");

            migrationBuilder.DropColumn(
                name: "IsFamilyDisease",
                table: "PatientDisease");

            migrationBuilder.DropColumn(
                name: "AtcCode",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "AtcVersion",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "WorkplaceId",
                table: "MedicalEncounters");

            migrationBuilder.AlterColumn<int>(
                name: "DiseaseStatus",
                table: "PatientDisease",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationUsage_MedicalEncounters_MedicationId",
                table: "MedicationUsage",
                column: "MedicationId",
                principalTable: "MedicalEncounters",
                principalColumn: "MedicalEncounterId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
