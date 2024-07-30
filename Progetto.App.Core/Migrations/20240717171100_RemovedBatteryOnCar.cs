using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemovedBatteryOnCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cars_ParkingSlots_ParkingSlotId",
                table: "Cars");

            migrationBuilder.DropIndex(
                name: "IX_Cars_ParkingSlotId",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "BatteryPercentage",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "ParkingSlotId",
                table: "Cars");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 19, 10, 54, 754, DateTimeKind.Local).AddTicks(3297),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 17, 1, 53, 44, 877, DateTimeKind.Local).AddTicks(3025));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 1, 53, 44, 877, DateTimeKind.Local).AddTicks(3025),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 17, 19, 10, 54, 754, DateTimeKind.Local).AddTicks(3297));

            migrationBuilder.AddColumn<decimal>(
                name: "BatteryPercentage",
                table: "Cars",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ParkingSlotId",
                table: "Cars",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cars_ParkingSlotId",
                table: "Cars",
                column: "ParkingSlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cars_ParkingSlots_ParkingSlotId",
                table: "Cars",
                column: "ParkingSlotId",
                principalTable: "ParkingSlots",
                principalColumn: "Id");
        }
    }
}
