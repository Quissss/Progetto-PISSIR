using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddParkingMwBotFixChargeHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParkingId",
                table: "MWBots",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TargetChargePercentage",
                table: "CurrentlyCharging",
                type: "decimal(5, 2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 22, 1, 20, 925, DateTimeKind.Local).AddTicks(5884),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 17, 19, 18, 52, 515, DateTimeKind.Local).AddTicks(9028));

            migrationBuilder.AlterColumn<decimal>(
                name: "StartChargePercentage",
                table: "CurrentlyCharging",
                type: "decimal(5, 2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EnergyConsumed",
                table: "CurrentlyCharging",
                type: "decimal(5, 2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "CurrentlyCharging",
                type: "decimal(5, 2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "TargetChargePercentage",
                table: "ChargeHistory",
                type: "decimal(5, 2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 22, 1, 20, 924, DateTimeKind.Local).AddTicks(5587),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 17, 19, 18, 52, 514, DateTimeKind.Local).AddTicks(8921));

            migrationBuilder.AlterColumn<decimal>(
                name: "StartChargePercentage",
                table: "ChargeHistory",
                type: "decimal(5, 2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 22, 1, 20, 924, DateTimeKind.Local).AddTicks(6144),
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AddColumn<decimal>(
                name: "EnergyConsumed",
                table: "ChargeHistory",
                type: "decimal(5, 2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "ChargeHistory",
                type: "decimal(5, 2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_MWBots_ParkingId",
                table: "MWBots",
                column: "ParkingId");

            migrationBuilder.AddForeignKey(
                name: "FK_MWBots_Parkings_ParkingId",
                table: "MWBots",
                column: "ParkingId",
                principalTable: "Parkings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MWBots_Parkings_ParkingId",
                table: "MWBots");

            migrationBuilder.DropIndex(
                name: "IX_MWBots_ParkingId",
                table: "MWBots");

            migrationBuilder.DropColumn(
                name: "ParkingId",
                table: "MWBots");

            migrationBuilder.DropColumn(
                name: "EnergyConsumed",
                table: "CurrentlyCharging");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "CurrentlyCharging");

            migrationBuilder.DropColumn(
                name: "EnergyConsumed",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "ChargeHistory");

            migrationBuilder.AlterColumn<decimal>(
                name: "TargetChargePercentage",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5, 2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 19, 18, 52, 515, DateTimeKind.Local).AddTicks(9028),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 17, 22, 1, 20, 925, DateTimeKind.Local).AddTicks(5884));

            migrationBuilder.AlterColumn<decimal>(
                name: "StartChargePercentage",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5, 2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TargetChargePercentage",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5, 2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 19, 18, 52, 514, DateTimeKind.Local).AddTicks(8921),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 17, 22, 1, 20, 924, DateTimeKind.Local).AddTicks(5587));

            migrationBuilder.AlterColumn<decimal>(
                name: "StartChargePercentage",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5, 2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 17, 22, 1, 20, 924, DateTimeKind.Local).AddTicks(6144));
        }
    }
}
