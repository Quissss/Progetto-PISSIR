using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddParkingSlotIdOnCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stopover_Parkings_ParkingId",
                table: "Stopover");

            migrationBuilder.RenameColumn(
                name: "ParkingId",
                table: "Stopover",
                newName: "ParkingSlotId");

            migrationBuilder.RenameIndex(
                name: "IX_Stopover_ParkingId",
                table: "Stopover",
                newName: "IX_Stopover_ParkingSlotId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 25, 0, 39, 12, 294, DateTimeKind.Local).AddTicks(4197),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 24, 23, 27, 49, 234, DateTimeKind.Local).AddTicks(7405));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 25, 0, 39, 12, 290, DateTimeKind.Local).AddTicks(6709),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 24, 23, 27, 49, 231, DateTimeKind.Local).AddTicks(3008));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 25, 0, 39, 12, 289, DateTimeKind.Local).AddTicks(5539),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 24, 23, 27, 49, 230, DateTimeKind.Local).AddTicks(3000));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 25, 0, 39, 12, 289, DateTimeKind.Local).AddTicks(6102),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 24, 23, 27, 49, 230, DateTimeKind.Local).AddTicks(3523));

            migrationBuilder.AddColumn<int>(
                name: "ParkingSlotId",
                table: "Cars",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Stopover_ParkingSlots_ParkingSlotId",
                table: "Stopover",
                column: "ParkingSlotId",
                principalTable: "ParkingSlots",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stopover_ParkingSlots_ParkingSlotId",
                table: "Stopover");

            migrationBuilder.DropColumn(
                name: "ParkingSlotId",
                table: "Cars");

            migrationBuilder.RenameColumn(
                name: "ParkingSlotId",
                table: "Stopover",
                newName: "ParkingId");

            migrationBuilder.RenameIndex(
                name: "IX_Stopover_ParkingSlotId",
                table: "Stopover",
                newName: "IX_Stopover_ParkingId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 23, 27, 49, 234, DateTimeKind.Local).AddTicks(7405),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 25, 0, 39, 12, 294, DateTimeKind.Local).AddTicks(4197));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 23, 27, 49, 231, DateTimeKind.Local).AddTicks(3008),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 25, 0, 39, 12, 290, DateTimeKind.Local).AddTicks(6709));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 23, 27, 49, 230, DateTimeKind.Local).AddTicks(3000),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 25, 0, 39, 12, 289, DateTimeKind.Local).AddTicks(5539));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 23, 27, 49, 230, DateTimeKind.Local).AddTicks(3523),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 25, 0, 39, 12, 289, DateTimeKind.Local).AddTicks(6102));

            migrationBuilder.AddForeignKey(
                name: "FK_Stopover_Parkings_ParkingId",
                table: "Stopover",
                column: "ParkingId",
                principalTable: "Parkings",
                principalColumn: "Id");
        }
    }
}
