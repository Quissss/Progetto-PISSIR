using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddIRIsBeingHandledFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 16, 0, 57, 1, 996, DateTimeKind.Local).AddTicks(4409),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 6, 22, 35, 17, 460, DateTimeKind.Local).AddTicks(5453));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "PaymentHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 16, 0, 57, 1, 995, DateTimeKind.Local).AddTicks(1504),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 6, 22, 35, 17, 459, DateTimeKind.Local).AddTicks(2331));

            migrationBuilder.AddColumn<bool>(
                name: "IsBeingHandled",
                table: "ImmediateRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 16, 0, 57, 1, 992, DateTimeKind.Local).AddTicks(2869),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 6, 22, 35, 17, 456, DateTimeKind.Local).AddTicks(3669));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LatestLocation",
                table: "MWBots");

            migrationBuilder.DropColumn(
                name: "IsBeingHandled",
                table: "ImmediateRequests");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 6, 22, 35, 17, 460, DateTimeKind.Local).AddTicks(5453),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 16, 0, 57, 1, 996, DateTimeKind.Local).AddTicks(4409));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "PaymentHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 6, 22, 35, 17, 459, DateTimeKind.Local).AddTicks(2331),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 16, 0, 57, 1, 995, DateTimeKind.Local).AddTicks(1504));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 6, 22, 35, 17, 456, DateTimeKind.Local).AddTicks(3669),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 16, 0, 57, 1, 992, DateTimeKind.Local).AddTicks(2869));
        }
    }
}
