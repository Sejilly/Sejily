using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sejily.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_Diseases_Medications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorWorkplaces_Doctors_DoctorId",
                table: "DoctorWorkplaces");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorWorkplaces_Workplaces_WorkplaceId",
                table: "DoctorWorkplaces");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorWorkplaces",
                table: "DoctorWorkplaces");

            migrationBuilder.RenameTable(
                name: "DoctorWorkplaces",
                newName: "DoctorWorkplace");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorWorkplaces_WorkplaceId",
                table: "DoctorWorkplace",
                newName: "IX_DoctorWorkplace_WorkplaceId");

            migrationBuilder.AlterColumn<string>(
                name: "NationalID",
                table: "AspNetUsers",
                type: "nchar(14)",
                fixedLength: true,
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(14)",
                oldMaxLength: 14);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorWorkplace",
                table: "DoctorWorkplace",
                columns: new[] { "DoctorId", "WorkplaceId" });

            migrationBuilder.CreateTable(
                name: "Diseases",
                columns: table => new
                {
                    DiseaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IcdCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IcdVersion = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IcdUri = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "VARCHAR(200)", nullable: false),
                    IsGenetic = table.Column<bool>(type: "bit", nullable: false),
                    IsChronic = table.Column<bool>(type: "bit", nullable: false),
                    TitleAr = table.Column<string>(type: "NVARCHAR(200)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diseases", x => x.DiseaseId);
                    table.UniqueConstraint("AK_Diseases_IcdUri", x => x.IcdUri);
                });

            migrationBuilder.CreateTable(
                name: "MedicalEncounters",
                columns: table => new
                {
                    MedicalEncounterId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    WorkplacesId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalEncounters", x => x.MedicalEncounterId);
                    table.UniqueConstraint("AK_MedicalEncounters_DoctorId_DateTime", x => new { x.DoctorId, x.DateTime });
                    table.UniqueConstraint("AK_MedicalEncounters_PatientId_DateTime", x => new { x.PatientId, x.DateTime });
                    table.ForeignKey(
                        name: "FK_MedicalEncounters_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalEncounters_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    MedicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActiveIngredient = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Form = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.MedicationId);
                });

            migrationBuilder.CreateTable(
                name: "PatientDisease",
                columns: table => new
                {
                    PatientDiseaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DiseaseId = table.Column<int>(type: "int", nullable: false),
                    MedicalEncounterId = table.Column<int>(type: "int", nullable: true),
                    DiseaseStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientDisease", x => x.PatientDiseaseId);
                    table.ForeignKey(
                        name: "FK_PatientDisease_Diseases_DiseaseId",
                        column: x => x.DiseaseId,
                        principalTable: "Diseases",
                        principalColumn: "DiseaseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientDisease_MedicalEncounters_MedicalEncounterId",
                        column: x => x.MedicalEncounterId,
                        principalTable: "MedicalEncounters",
                        principalColumn: "MedicalEncounterId");
                    table.ForeignKey(
                        name: "FK_PatientDisease_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicationUsage",
                columns: table => new
                {
                    MedicationUsageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicationId = table.Column<int>(type: "int", nullable: false),
                    MedicalEncounterId = table.Column<int>(type: "int", nullable: false),
                    Dose_Amount = table.Column<decimal>(type: "DECIMAL(18,4)", nullable: false),
                    Dose_Unit = table.Column<int>(type: "int", nullable: false),
                    Frequency_Times = table.Column<int>(type: "int", nullable: false),
                    Frequency_Unit = table.Column<int>(type: "int", nullable: false),
                    Duration_Amount = table.Column<int>(type: "int", nullable: false),
                    Duration_Unit = table.Column<int>(type: "int", nullable: false),
                    AdministrationRoute = table.Column<int>(type: "int", nullable: false),
                    MealRelation = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationUsage", x => x.MedicationUsageId);
                    table.ForeignKey(
                        name: "FK_MedicationUsage_MedicalEncounters_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "MedicalEncounters",
                        principalColumn: "MedicalEncounterId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicationUsage_Medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medications",
                        principalColumn: "MedicationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicationUsage_Patients_PatientUserId",
                        column: x => x.PatientUserId,
                        principalTable: "Patients",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Diseases_IcdVersion_IcdCode",
                table: "Diseases",
                columns: new[] { "IcdVersion", "IcdCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medications_ActiveIngredient",
                table: "Medications",
                column: "ActiveIngredient");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationUsage_MedicationId",
                table: "MedicationUsage",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationUsage_PatientUserId",
                table: "MedicationUsage",
                column: "PatientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientDisease_DiseaseId",
                table: "PatientDisease",
                column: "DiseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientDisease_MedicalEncounterId",
                table: "PatientDisease",
                column: "MedicalEncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientDisease_PatientId_DiseaseId_MedicalEncounterId",
                table: "PatientDisease",
                columns: new[] { "PatientId", "DiseaseId", "MedicalEncounterId" },
                unique: true,
                filter: "[MedicalEncounterId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorWorkplace_Doctors_DoctorId",
                table: "DoctorWorkplace",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorWorkplace_Workplaces_WorkplaceId",
                table: "DoctorWorkplace",
                column: "WorkplaceId",
                principalTable: "Workplaces",
                principalColumn: "WorkplaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorWorkplace_Doctors_DoctorId",
                table: "DoctorWorkplace");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorWorkplace_Workplaces_WorkplaceId",
                table: "DoctorWorkplace");

            migrationBuilder.DropTable(
                name: "MedicationUsage");

            migrationBuilder.DropTable(
                name: "PatientDisease");

            migrationBuilder.DropTable(
                name: "Medications");

            migrationBuilder.DropTable(
                name: "Diseases");

            migrationBuilder.DropTable(
                name: "MedicalEncounters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorWorkplace",
                table: "DoctorWorkplace");

            migrationBuilder.RenameTable(
                name: "DoctorWorkplace",
                newName: "DoctorWorkplaces");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorWorkplace_WorkplaceId",
                table: "DoctorWorkplaces",
                newName: "IX_DoctorWorkplaces_WorkplaceId");

            migrationBuilder.AlterColumn<string>(
                name: "NationalID",
                table: "AspNetUsers",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nchar(14)",
                oldFixedLength: true,
                oldMaxLength: 14);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorWorkplaces",
                table: "DoctorWorkplaces",
                columns: new[] { "DoctorId", "WorkplaceId" });

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorWorkplaces_Doctors_DoctorId",
                table: "DoctorWorkplaces",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorWorkplaces_Workplaces_WorkplaceId",
                table: "DoctorWorkplaces",
                column: "WorkplaceId",
                principalTable: "Workplaces",
                principalColumn: "WorkplaceId");
        }
    }
}
