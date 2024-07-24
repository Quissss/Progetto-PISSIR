using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class CarIsInsideFlagOnReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopOverTime",
                table: "StopOver",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 13, 25, 18, 882, DateTimeKind.Local).AddTicks(8532),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 86, DateTimeKind.Local).AddTicks(4603));

            migrationBuilder.AddColumn<bool>(
                name: "CarIsInside",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 13, 25, 18, 831, DateTimeKind.Local).AddTicks(7654),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 52, DateTimeKind.Local).AddTicks(8783));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 13, 25, 18, 828, DateTimeKind.Local).AddTicks(2092),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 30, DateTimeKind.Local).AddTicks(9003));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 13, 25, 18, 828, DateTimeKind.Local).AddTicks(3811),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 31, DateTimeKind.Local).AddTicks(47));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarIsInside",
                table: "Reservations");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopOverTime",
                table: "StopOver",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 86, DateTimeKind.Local).AddTicks(4603),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 24, 13, 25, 18, 882, DateTimeKind.Local).AddTicks(8532));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 52, DateTimeKind.Local).AddTicks(8783),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 24, 13, 25, 18, 831, DateTimeKind.Local).AddTicks(7654));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 30, DateTimeKind.Local).AddTicks(9003),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 24, 13, 25, 18, 828, DateTimeKind.Local).AddTicks(2092));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 31, DateTimeKind.Local).AddTicks(47),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 24, 13, 25, 18, 828, DateTimeKind.Local).AddTicks(3811));
        }
    }
}
