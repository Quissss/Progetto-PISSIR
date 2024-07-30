using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAttributesCurrentlyCharging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentlyCharging_AspNetUsers_UserId",
                table: "CurrentlyCharging");

            migrationBuilder.DropForeignKey(
                name: "FK_CurrentlyCharging_Cars_CarPlate",
                table: "CurrentlyCharging");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "CurrentlyCharging");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "CarPlate",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "ParkingSlotId",
                table: "CurrentlyCharging",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StartChargePercentage",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 1, 53, 44, 877, DateTimeKind.Local).AddTicks(3025));

            migrationBuilder.AddColumn<decimal>(
                name: "TargetChargePercentage",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrentlyCharging_ParkingSlotId",
                table: "CurrentlyCharging",
                column: "ParkingSlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentlyCharging_AspNetUsers_UserId",
                table: "CurrentlyCharging",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentlyCharging_Cars_CarPlate",
                table: "CurrentlyCharging",
                column: "CarPlate",
                principalTable: "Cars",
                principalColumn: "LicencePlate");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentlyCharging_ParkingSlots_ParkingSlotId",
                table: "CurrentlyCharging",
                column: "ParkingSlotId",
                principalTable: "ParkingSlots",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentlyCharging_AspNetUsers_UserId",
                table: "CurrentlyCharging");

            migrationBuilder.DropForeignKey(
                name: "FK_CurrentlyCharging_Cars_CarPlate",
                table: "CurrentlyCharging");

            migrationBuilder.DropForeignKey(
                name: "FK_CurrentlyCharging_ParkingSlots_ParkingSlotId",
                table: "CurrentlyCharging");

            migrationBuilder.DropIndex(
                name: "IX_CurrentlyCharging_ParkingSlotId",
                table: "CurrentlyCharging");

            migrationBuilder.DropColumn(
                name: "ParkingSlotId",
                table: "CurrentlyCharging");

            migrationBuilder.DropColumn(
                name: "StartChargePercentage",
                table: "CurrentlyCharging");

            migrationBuilder.DropColumn(
                name: "StartChargingTime",
                table: "CurrentlyCharging");

            migrationBuilder.DropColumn(
                name: "TargetChargePercentage",
                table: "CurrentlyCharging");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CarPlate",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 0, 10, 13, 30, DateTimeKind.Local).AddTicks(8217));

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentlyCharging_AspNetUsers_UserId",
                table: "CurrentlyCharging",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentlyCharging_Cars_CarPlate",
                table: "CurrentlyCharging",
                column: "CarPlate",
                principalTable: "Cars",
                principalColumn: "LicencePlate",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
