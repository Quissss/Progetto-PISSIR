using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class Add_PaymentTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 29, 19, 13, 23, 627, DateTimeKind.Local).AddTicks(8139),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 26, 19, 2, 7, 33, DateTimeKind.Local).AddTicks(9487));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "PaymentHistory",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 26, 19, 2, 7, 32, DateTimeKind.Local).AddTicks(5011));

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "PaymentHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 29, 19, 13, 23, 626, DateTimeKind.Local).AddTicks(5439));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 29, 19, 13, 23, 623, DateTimeKind.Local).AddTicks(7961),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 26, 19, 2, 7, 29, DateTimeKind.Local).AddTicks(3022));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "PaymentHistory");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 26, 19, 2, 7, 33, DateTimeKind.Local).AddTicks(9487),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 29, 19, 13, 23, 627, DateTimeKind.Local).AddTicks(8139));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "PaymentHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 26, 19, 2, 7, 32, DateTimeKind.Local).AddTicks(5011),
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 26, 19, 2, 7, 29, DateTimeKind.Local).AddTicks(3022),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 29, 19, 13, 23, 623, DateTimeKind.Local).AddTicks(7961));
        }
    }
}
