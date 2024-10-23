using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class Add_IR_IsBeingHandledFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 22, 0, 56, 20, 290, DateTimeKind.Local).AddTicks(5181),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 16, 23, 55, 24, 501, DateTimeKind.Local).AddTicks(1450));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "PaymentHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 22, 0, 56, 20, 289, DateTimeKind.Local).AddTicks(5939),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 16, 23, 55, 24, 500, DateTimeKind.Local).AddTicks(3260));

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
                defaultValue: new DateTime(2024, 10, 22, 0, 56, 20, 286, DateTimeKind.Local).AddTicks(8135),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 16, 23, 55, 24, 497, DateTimeKind.Local).AddTicks(8328));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBeingHandled",
                table: "ImmediateRequests");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 16, 23, 55, 24, 501, DateTimeKind.Local).AddTicks(1450),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 22, 0, 56, 20, 290, DateTimeKind.Local).AddTicks(5181));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "PaymentHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 16, 23, 55, 24, 500, DateTimeKind.Local).AddTicks(3260),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 22, 0, 56, 20, 289, DateTimeKind.Local).AddTicks(5939));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 16, 23, 55, 24, 497, DateTimeKind.Local).AddTicks(8328),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 22, 0, 56, 20, 286, DateTimeKind.Local).AddTicks(8135));
        }
    }
}
