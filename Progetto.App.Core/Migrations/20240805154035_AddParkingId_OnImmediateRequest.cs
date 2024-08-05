using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddParkingId_OnImmediateRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 8, 5, 17, 40, 32, 462, DateTimeKind.Local).AddTicks(947),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 29, 19, 13, 23, 627, DateTimeKind.Local).AddTicks(8139));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "PaymentHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 8, 5, 17, 40, 32, 460, DateTimeKind.Local).AddTicks(7498),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 29, 19, 13, 23, 626, DateTimeKind.Local).AddTicks(5439));

            migrationBuilder.AddColumn<int>(
                name: "ParkingId",
                table: "ImmediateRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 8, 5, 17, 40, 32, 457, DateTimeKind.Local).AddTicks(8690),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 29, 19, 13, 23, 623, DateTimeKind.Local).AddTicks(7961));

            migrationBuilder.CreateIndex(
                name: "IX_ImmediateRequests_ParkingId",
                table: "ImmediateRequests",
                column: "ParkingId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImmediateRequests_Parkings_ParkingId",
                table: "ImmediateRequests",
                column: "ParkingId",
                principalTable: "Parkings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImmediateRequests_Parkings_ParkingId",
                table: "ImmediateRequests");

            migrationBuilder.DropIndex(
                name: "IX_ImmediateRequests_ParkingId",
                table: "ImmediateRequests");

            migrationBuilder.DropColumn(
                name: "ParkingId",
                table: "ImmediateRequests");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 29, 19, 13, 23, 627, DateTimeKind.Local).AddTicks(8139),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 8, 5, 17, 40, 32, 462, DateTimeKind.Local).AddTicks(947));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "PaymentHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 29, 19, 13, 23, 626, DateTimeKind.Local).AddTicks(5439),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 8, 5, 17, 40, 32, 460, DateTimeKind.Local).AddTicks(7498));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 29, 19, 13, 23, 623, DateTimeKind.Local).AddTicks(7961),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 8, 5, 17, 40, 32, 457, DateTimeKind.Local).AddTicks(8690));
        }
    }
}
