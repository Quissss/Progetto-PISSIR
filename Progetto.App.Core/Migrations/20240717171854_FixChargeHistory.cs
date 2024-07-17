using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class FixChargeHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChargeHistory_AspNetUsers_UserId",
                table: "ChargeHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_ChargeHistory_Cars_CarLicencePlate",
                table: "ChargeHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_ChargeHistory_MWBots_MWBotId",
                table: "ChargeHistory");

            migrationBuilder.DropIndex(
                name: "IX_ChargeHistory_CarLicencePlate",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "CarLicencePlate",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "ChargeStartDate",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "EndChargeLevel",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "ParkEndDate",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "ParkStartDate",
                table: "ChargeHistory");

            migrationBuilder.RenameColumn(
                name: "MWBotId",
                table: "ChargeHistory",
                newName: "MwBotId");

            migrationBuilder.RenameColumn(
                name: "StartChargeLevel",
                table: "ChargeHistory",
                newName: "EndChargingTime");

            migrationBuilder.RenameColumn(
                name: "ChargeEndDate",
                table: "ChargeHistory",
                newName: "TargetChargePercentage");

            migrationBuilder.RenameIndex(
                name: "IX_ChargeHistory_MWBotId",
                table: "ChargeHistory",
                newName: "IX_ChargeHistory_MwBotId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 19, 18, 52, 515, DateTimeKind.Local).AddTicks(9028),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 17, 19, 10, 54, 754, DateTimeKind.Local).AddTicks(3297));

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "MwBotId",
                table: "ChargeHistory",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CarPlate",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParkingSlotId",
                table: "ChargeHistory",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StartChargePercentage",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 19, 18, 52, 514, DateTimeKind.Local).AddTicks(8921));

            migrationBuilder.CreateIndex(
                name: "IX_ChargeHistory_CarPlate",
                table: "ChargeHistory",
                column: "CarPlate");

            migrationBuilder.CreateIndex(
                name: "IX_ChargeHistory_ParkingSlotId",
                table: "ChargeHistory",
                column: "ParkingSlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargeHistory_AspNetUsers_UserId",
                table: "ChargeHistory",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargeHistory_Cars_CarPlate",
                table: "ChargeHistory",
                column: "CarPlate",
                principalTable: "Cars",
                principalColumn: "LicencePlate");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargeHistory_MWBots_MwBotId",
                table: "ChargeHistory",
                column: "MwBotId",
                principalTable: "MWBots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChargeHistory_ParkingSlots_ParkingSlotId",
                table: "ChargeHistory",
                column: "ParkingSlotId",
                principalTable: "ParkingSlots",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChargeHistory_AspNetUsers_UserId",
                table: "ChargeHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_ChargeHistory_Cars_CarPlate",
                table: "ChargeHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_ChargeHistory_MWBots_MwBotId",
                table: "ChargeHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_ChargeHistory_ParkingSlots_ParkingSlotId",
                table: "ChargeHistory");

            migrationBuilder.DropIndex(
                name: "IX_ChargeHistory_CarPlate",
                table: "ChargeHistory");

            migrationBuilder.DropIndex(
                name: "IX_ChargeHistory_ParkingSlotId",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "CarPlate",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "ParkingSlotId",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "StartChargePercentage",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "StartChargingTime",
                table: "ChargeHistory");

            migrationBuilder.RenameColumn(
                name: "MwBotId",
                table: "ChargeHistory",
                newName: "MWBotId");

            migrationBuilder.RenameColumn(
                name: "TargetChargePercentage",
                table: "ChargeHistory",
                newName: "ChargeEndDate");

            migrationBuilder.RenameColumn(
                name: "EndChargingTime",
                table: "ChargeHistory",
                newName: "StartChargeLevel");

            migrationBuilder.RenameIndex(
                name: "IX_ChargeHistory_MwBotId",
                table: "ChargeHistory",
                newName: "IX_ChargeHistory_MWBotId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 19, 10, 54, 754, DateTimeKind.Local).AddTicks(3297),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 17, 19, 18, 52, 515, DateTimeKind.Local).AddTicks(9028));

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MWBotId",
                table: "ChargeHistory",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "CarLicencePlate",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ChargeStartDate",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "EndChargeLevel",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ParkEndDate",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ParkStartDate",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_ChargeHistory_CarLicencePlate",
                table: "ChargeHistory",
                column: "CarLicencePlate");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargeHistory_AspNetUsers_UserId",
                table: "ChargeHistory",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChargeHistory_Cars_CarLicencePlate",
                table: "ChargeHistory",
                column: "CarLicencePlate",
                principalTable: "Cars",
                principalColumn: "LicencePlate",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChargeHistory_MWBots_MWBotId",
                table: "ChargeHistory",
                column: "MWBotId",
                principalTable: "MWBots",
                principalColumn: "Id");
        }
    }
}
