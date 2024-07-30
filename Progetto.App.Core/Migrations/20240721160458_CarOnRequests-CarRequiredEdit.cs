using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class CarOnRequestsCarRequiredEdit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CarPlate",
                table: "Reservations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CarLicencePlate",
                table: "ImmediateRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CarPlate",
                table: "ImmediateRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 18, 4, 55, 437, DateTimeKind.Local).AddTicks(9428),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 21, 2, 51, 1, 323, DateTimeKind.Local).AddTicks(2548));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 18, 4, 55, 436, DateTimeKind.Local).AddTicks(9456),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 21, 2, 51, 1, 321, DateTimeKind.Local).AddTicks(8571));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 18, 4, 55, 436, DateTimeKind.Local).AddTicks(9970),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 21, 2, 51, 1, 321, DateTimeKind.Local).AddTicks(9315));

            migrationBuilder.AlterColumn<bool>(
                name: "IsElectric",
                table: "Cars",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CarPlate",
                table: "Reservations",
                column: "CarPlate");

            migrationBuilder.CreateIndex(
                name: "IX_ImmediateRequests_CarLicencePlate",
                table: "ImmediateRequests",
                column: "CarLicencePlate");

            migrationBuilder.AddForeignKey(
                name: "FK_ImmediateRequests_Cars_CarLicencePlate",
                table: "ImmediateRequests",
                column: "CarLicencePlate",
                principalTable: "Cars",
                principalColumn: "LicencePlate");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Cars_CarPlate",
                table: "Reservations",
                column: "CarPlate",
                principalTable: "Cars",
                principalColumn: "LicencePlate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImmediateRequests_Cars_CarLicencePlate",
                table: "ImmediateRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Cars_CarPlate",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CarPlate",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_ImmediateRequests_CarLicencePlate",
                table: "ImmediateRequests");

            migrationBuilder.DropColumn(
                name: "CarPlate",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CarLicencePlate",
                table: "ImmediateRequests");

            migrationBuilder.DropColumn(
                name: "CarPlate",
                table: "ImmediateRequests");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 2, 51, 1, 323, DateTimeKind.Local).AddTicks(2548),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 21, 18, 4, 55, 437, DateTimeKind.Local).AddTicks(9428));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 2, 51, 1, 321, DateTimeKind.Local).AddTicks(8571),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 21, 18, 4, 55, 436, DateTimeKind.Local).AddTicks(9456));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 2, 51, 1, 321, DateTimeKind.Local).AddTicks(9315),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 21, 18, 4, 55, 436, DateTimeKind.Local).AddTicks(9970));

            migrationBuilder.AlterColumn<bool>(
                name: "IsElectric",
                table: "Cars",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);
        }
    }
}
