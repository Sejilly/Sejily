using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sejily.API.Migrations
{
    /// <inheritdoc />
    public partial class AccessRequestHandling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_EmergencyReviews_Response_RespondedAt",
                table: "EmergencyReviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AccessRequests_Decision_DecidedAt",
                table: "AccessRequests");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Accesses_Permanent_Expiry",
                table: "Accesses");

            migrationBuilder.AddColumn<Guid>(
                name: "SourceOutboxId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TreatmentLengthDays",
                table: "MedicalEncounters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTransitionAt",
                table: "AccessRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectionRequestedAt",
                table: "AccessRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FirstExaminationDate",
                table: "Accesses",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TreatmentLengthDays",
                table: "Accesses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccessNotificationOutbox",
                columns: table => new
                {
                    OutboxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RecipientUserId = table.Column<int>(type: "int", nullable: false),
                    SubjectPatientId = table.Column<int>(type: "int", nullable: true),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessNotificationOutbox", x => x.OutboxId);
                    table.ForeignKey(
                        name: "FK_AccessNotificationOutbox_AccessRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "AccessRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessNotificationOutbox_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessNotificationOutbox_Patients_SubjectPatientId",
                        column: x => x.SubjectPatientId,
                        principalTable: "Patients",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SourceOutboxId",
                table: "Notifications",
                column: "SourceOutboxId",
                unique: true,
                filter: "[SourceOutboxId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EmergencyReviews_Response_RespondedAt",
                table: "EmergencyReviews",
                sql: "(PatientResponse IS NULL AND RespondedAt IS NULL)\nOR\n(PatientResponse IS NOT NULL AND RespondedAt IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AccessRequests_Decision_DecidedAt",
                table: "AccessRequests",
                sql: "(Decision IS NULL AND DecidedAt IS NULL)\nOR\n(Decision IS NOT NULL AND DecidedAt IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Accesses_Permanent_Expiry",
                table: "Accesses",
                sql: "(ValidityType = 'Permanent' AND ExpiresAt IS NULL)\nOR\n(ValidityType <> 'Permanent' AND ExpiresAt IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_AccessNotificationOutbox_ProcessedAt_OccurredAt",
                table: "AccessNotificationOutbox",
                columns: new[] { "ProcessedAt", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessNotificationOutbox_RecipientUserId",
                table: "AccessNotificationOutbox",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessNotificationOutbox_RequestId",
                table: "AccessNotificationOutbox",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessNotificationOutbox_SubjectPatientId",
                table: "AccessNotificationOutbox",
                column: "SubjectPatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessNotificationOutbox");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_SourceOutboxId",
                table: "Notifications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EmergencyReviews_Response_RespondedAt",
                table: "EmergencyReviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AccessRequests_Decision_DecidedAt",
                table: "AccessRequests");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Accesses_Permanent_Expiry",
                table: "Accesses");

            migrationBuilder.DropColumn(
                name: "SourceOutboxId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TreatmentLengthDays",
                table: "MedicalEncounters");

            migrationBuilder.DropColumn(
                name: "LastTransitionAt",
                table: "AccessRequests");

            migrationBuilder.DropColumn(
                name: "RejectionRequestedAt",
                table: "AccessRequests");

            migrationBuilder.DropColumn(
                name: "FirstExaminationDate",
                table: "Accesses");

            migrationBuilder.DropColumn(
                name: "TreatmentLengthDays",
                table: "Accesses");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EmergencyReviews_Response_RespondedAt",
                table: "EmergencyReviews",
                sql: "(PatientResponse IS NULL AND RespondedAt IS NULL)\r\nOR\r\n(PatientResponse IS NOT NULL AND RespondedAt IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AccessRequests_Decision_DecidedAt",
                table: "AccessRequests",
                sql: "(Decision IS NULL AND DecidedAt IS NULL)\r\nOR\r\n(Decision IS NOT NULL AND DecidedAt IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Accesses_Permanent_Expiry",
                table: "Accesses",
                sql: "(ValidityType = 'Permanent' AND ExpiresAt IS NULL)\r\nOR\r\n(ValidityType <> 'Permanent' AND ExpiresAt IS NOT NULL)");
        }
    }
}
