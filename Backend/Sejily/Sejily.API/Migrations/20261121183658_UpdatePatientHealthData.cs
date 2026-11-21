using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sejily.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePatientHealthData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PatientDisease_PatientId_DiseaseId_IsFamilyDisease",
                table: "PatientDisease");

            migrationBuilder.DropColumn(
                name: "IsFamilyDisease",
                table: "PatientDisease");

            migrationBuilder.RenameColumn(
                name: "IcdVersion",
                table: "Diseases",
                newName: "IcdCode_Version");

            migrationBuilder.RenameColumn(
                name: "IcdCode",
                table: "Diseases",
                newName: "IcdCode_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Diseases_IcdVersion_IcdCode",
                table: "Diseases",
                newName: "IX_Diseases_IcdCode_Version_IcdCode_Code");

            migrationBuilder.AddColumn<string>(
                name: "PatientEntryMedications",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "DiseaseEntryName",
                table: "PatientDisease",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAllergy",
                table: "Diseases",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PatientFamilyDisease",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DiseaseId = table.Column<int>(type: "int", nullable: false),
                    DiseaseEntryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FamilyRelationship = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientFamilyDisease", x => new { x.PatientId, x.DiseaseId });
                    table.ForeignKey(
                        name: "FK_PatientFamilyDisease_Diseases_DiseaseId",
                        column: x => x.DiseaseId,
                        principalTable: "Diseases",
                        principalColumn: "DiseaseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientFamilyDisease_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientDisease_PatientId_DiseaseId",
                table: "PatientDisease",
                columns: new[] { "PatientId", "DiseaseId" },
                unique: true,
                filter: "[MedicalEncounterId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PatientFamilyDisease_DiseaseId",
                table: "PatientFamilyDisease",
                column: "DiseaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientFamilyDisease");

            migrationBuilder.DropIndex(
                name: "IX_PatientDisease_PatientId_DiseaseId",
                table: "PatientDisease");

            migrationBuilder.DropColumn(
                name: "PatientEntryMedications",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "DiseaseEntryName",
                table: "PatientDisease");

            migrationBuilder.DropColumn(
                name: "IsAllergy",
                table: "Diseases");

            migrationBuilder.RenameColumn(
                name: "IcdCode_Version",
                table: "Diseases",
                newName: "IcdVersion");

            migrationBuilder.RenameColumn(
                name: "IcdCode_Code",
                table: "Diseases",
                newName: "IcdCode");

            migrationBuilder.RenameIndex(
                name: "IX_Diseases_IcdCode_Version_IcdCode_Code",
                table: "Diseases",
                newName: "IX_Diseases_IcdVersion_IcdCode");

            migrationBuilder.AddColumn<bool>(
                name: "IsFamilyDisease",
                table: "PatientDisease",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PatientDisease_PatientId_DiseaseId_IsFamilyDisease",
                table: "PatientDisease",
                columns: new[] { "PatientId", "DiseaseId", "IsFamilyDisease" },
                unique: true,
                filter: "[MedicalEncounterId] IS NULL");
        }
    }
}
