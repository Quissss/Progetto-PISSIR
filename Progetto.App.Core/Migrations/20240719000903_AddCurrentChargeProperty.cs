using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentChargeProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MWBots_ParkingId",
                table: "MWBots");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 19, 2, 9, 0, 697, DateTimeKind.Local).AddTicks(2831),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 17, 22, 1, 20, 925, DateTimeKind.Local).AddTicks(5884));

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentChargePercentage",
                table: "CurrentlyCharging",
                type: "decimal(5, 2)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 19, 2, 9, 0, 696, DateTimeKind.Local).AddTicks(2839),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 17, 22, 1, 20, 924, DateTimeKind.Local).AddTicks(5587));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 19, 2, 9, 0, 696, DateTimeKind.Local).AddTicks(3372),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 17, 22, 1, 20, 924, DateTimeKind.Local).AddTicks(6144));

            migrationBuilder.CreateIndex(
                name: "IX_MWBots_ParkingId",
                table: "MWBots",
                column: "ParkingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MWBots_ParkingId",
                table: "MWBots");

            migrationBuilder.DropColumn(
                name: "CurrentChargePercentage",
                table: "CurrentlyCharging");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 22, 1, 20, 925, DateTimeKind.Local).AddTicks(5884),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 19, 2, 9, 0, 697, DateTimeKind.Local).AddTicks(2831));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 22, 1, 20, 924, DateTimeKind.Local).AddTicks(5587),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 19, 2, 9, 0, 696, DateTimeKind.Local).AddTicks(2839));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 22, 1, 20, 924, DateTimeKind.Local).AddTicks(6144),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 19, 2, 9, 0, 696, DateTimeKind.Local).AddTicks(3372));

            migrationBuilder.CreateIndex(
                name: "IX_MWBots_ParkingId",
                table: "MWBots",
                column: "ParkingId");
        }
    }
}
