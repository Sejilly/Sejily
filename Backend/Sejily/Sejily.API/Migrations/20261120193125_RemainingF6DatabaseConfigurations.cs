using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sejily.API.Migrations
{
    /// <inheritdoc />
    public partial class RemainingF6DatabaseConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    AccessId = table.Column<int>(type: "int", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    SubjectPatientId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    WorkplaceId = table.Column<int>(type: "int", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AccessRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "AccessRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Accesses_AccessId",
                        column: x => x.AccessId,
                        principalTable: "Accesses",
                        principalColumn: "AccessId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Patients_SubjectPatientId",
                        column: x => x.SubjectPatientId,
                        principalTable: "Patients",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Workplaces_WorkplaceId",
                        column: x => x.WorkplaceId,
                        principalTable: "Workplaces",
                        principalColumn: "WorkplaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceTokens",
                columns: table => new
                {
                    DeviceTokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTokens", x => x.DeviceTokenId);
                    table.ForeignKey(
                        name: "FK_DeviceTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientUserId = table.Column<int>(type: "int", nullable: false),
                    SubjectPatientId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_AccessRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "AccessRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Patients_SubjectPatientId",
                        column: x => x.SubjectPatientId,
                        principalTable: "Patients",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AccessId",
                table: "AuditLogs",
                column: "AccessId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorUserId",
                table: "AuditLogs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_RequestId_OccurredAt",
                table: "AuditLogs",
                columns: new[] { "RequestId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_SubjectPatientId_OccurredAt",
                table: "AuditLogs",
                columns: new[] { "SubjectPatientId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_WorkplaceId",
                table: "AuditLogs",
                column: "WorkplaceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_Token",
                table: "DeviceTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_UserId",
                table: "DeviceTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId_CreatedAt_NotificationId",
                table: "Notifications",
                columns: new[] { "RecipientUserId", "CreatedAt", "NotificationId" },
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId_IsRead",
                table: "Notifications",
                columns: new[] { "RecipientUserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RequestId",
                table: "Notifications",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SubjectPatientId",
                table: "Notifications",
                column: "SubjectPatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DeviceTokens");

            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
