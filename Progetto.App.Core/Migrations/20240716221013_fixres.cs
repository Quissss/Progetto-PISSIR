using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class fixres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ParkingSlots_ParkingSlotId",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "ParkingSlotId",
                table: "Reservations",
                newName: "ParkingId");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_ParkingSlotId",
                table: "Reservations",
                newName: "IX_Reservations_ParkingId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 17, 0, 10, 13, 30, DateTimeKind.Local).AddTicks(8217),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 16, 1, 51, 21, 220, DateTimeKind.Local).AddTicks(6351));

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Parkings_ParkingId",
                table: "Reservations",
                column: "ParkingId",
                principalTable: "Parkings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Parkings_ParkingId",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "ParkingId",
                table: "Reservations",
                newName: "ParkingSlotId");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_ParkingId",
                table: "Reservations",
                newName: "IX_Reservations_ParkingSlotId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 16, 1, 51, 21, 220, DateTimeKind.Local).AddTicks(6351),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 17, 0, 10, 13, 30, DateTimeKind.Local).AddTicks(8217));

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ParkingSlots_ParkingSlotId",
                table: "Reservations",
                column: "ParkingSlotId",
                principalTable: "ParkingSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
