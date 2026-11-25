using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sejily.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessRequestTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    WorkplaceId = table.Column<int>(type: "int", nullable: false),
                    AccessType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecipientPatientId = table.Column<int>(type: "int", nullable: false),
                    RespondedByUserId = table.Column<int>(type: "int", nullable: true),
                    Decision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupersededByRequestId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    ExpiryProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessRequests", x => x.RequestId);
                    table.CheckConstraint("CK_AccessRequests_Decision_DecidedAt", "(Decision IS NULL AND DecidedAt IS NULL)\r\nOR\r\n(Decision IS NOT NULL AND DecidedAt IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_AccessRequests_AccessRequests_SupersededByRequestId",
                        column: x => x.SupersededByRequestId,
                        principalTable: "AccessRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessRequests_AspNetUsers_RespondedByUserId",
                        column: x => x.RespondedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessRequests_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessRequests_Patients_RecipientPatientId",
                        column: x => x.RecipientPatientId,
                        principalTable: "Patients",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessRequests_Workplaces_WorkplaceId",
                        column: x => x.WorkplaceId,
                        principalTable: "Workplaces",
                        principalColumn: "WorkplaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Accesses",
                columns: table => new
                {
                    AccessId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    ValidityType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedByUserId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    ExpiryProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accesses", x => x.AccessId);
                    table.CheckConstraint("CK_Accesses_Expiry_After_Start", "ExpiresAt IS NULL OR ExpiresAt > StartAt");
                    table.CheckConstraint("CK_Accesses_Permanent_Expiry", "(ValidityType = 'Permanent' AND ExpiresAt IS NULL)\r\nOR\r\n(ValidityType <> 'Permanent' AND ExpiresAt IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Accesses_AccessRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "AccessRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accesses_AspNetUsers_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyReviews",
                columns: table => new
                {
                    AccessId = table.Column<int>(type: "int", nullable: false),
                    DoctorDeclaredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeclarationVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeclarationText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientResponse = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RespondedByUserId = table.Column<int>(type: "int", nullable: true),
                    EscalatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyReviews", x => x.AccessId);
                    table.CheckConstraint("CK_EmergencyReviews_Response_RespondedAt", "(PatientResponse IS NULL AND RespondedAt IS NULL)\r\nOR\r\n(PatientResponse IS NOT NULL AND RespondedAt IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_EmergencyReviews_Accesses_AccessId",
                        column: x => x.AccessId,
                        principalTable: "Accesses",
                        principalColumn: "AccessId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyReviews_AspNetUsers_RespondedByUserId",
                        column: x => x.RespondedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accesses_ClosedByUserId",
                table: "Accesses",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accesses_RequestId",
                table: "Accesses",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessRequests_DoctorId_SentAt",
                table: "AccessRequests",
                columns: new[] { "DoctorId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessRequests_DoctorId_WorkplaceId_RecipientPatientId",
                table: "AccessRequests",
                columns: new[] { "DoctorId", "WorkplaceId", "RecipientPatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessRequests_RecipientPatientId_SentAt",
                table: "AccessRequests",
                columns: new[] { "RecipientPatientId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessRequests_RespondedByUserId",
                table: "AccessRequests",
                column: "RespondedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessRequests_SupersededByRequestId",
                table: "AccessRequests",
                column: "SupersededByRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessRequests_WorkplaceId",
                table: "AccessRequests",
                column: "WorkplaceId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyReviews_RespondedByUserId",
                table: "EmergencyReviews",
                column: "RespondedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmergencyReviews");

            migrationBuilder.DropTable(
                name: "Accesses");

            migrationBuilder.DropTable(
                name: "AccessRequests");
        }
    }
}
