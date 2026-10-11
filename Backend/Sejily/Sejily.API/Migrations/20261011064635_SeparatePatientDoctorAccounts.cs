using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sejily.API.Migrations
{
    /// <inheritdoc />
    public partial class SeparatePatientDoctorAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_AspNetUsers_UserId",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_Patients_HealthCardNumber",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_NationalID",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "BloodType",
                table: "AspNetUsers",
                newName: "AccountType");

            migrationBuilder.AlterColumn<string>(
                name: "HealthCardNumber",
                table: "Patients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "BloodType",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_HealthCardNumber",
                table: "Patients",
                column: "HealthCardNumber",
                unique: true,
                filter: "[HealthCardNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_NationalID_AccountType",
                table: "AspNetUsers",
                columns: new[] { "NationalID", "AccountType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_AspNetUsers_UserId",
                table: "Doctors",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_AspNetUsers_UserId",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_Patients_HealthCardNumber",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_NationalID_AccountType",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BloodType",
                table: "Patients");

            migrationBuilder.RenameColumn(
                name: "AccountType",
                table: "AspNetUsers",
                newName: "BloodType");

            migrationBuilder.AlterColumn<string>(
                name: "HealthCardNumber",
                table: "Patients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_HealthCardNumber",
                table: "Patients",
                column: "HealthCardNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_NationalID",
                table: "AspNetUsers",
                column: "NationalID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_AspNetUsers_UserId",
                table: "Doctors",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
