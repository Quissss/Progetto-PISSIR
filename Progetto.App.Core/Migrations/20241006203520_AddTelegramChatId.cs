using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramChatId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 6, 22, 35, 17, 460, DateTimeKind.Local).AddTicks(5453),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 1, 2, 31, 26, 812, DateTimeKind.Local).AddTicks(3628));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "PaymentHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 6, 22, 35, 17, 459, DateTimeKind.Local).AddTicks(2331),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 1, 2, 31, 26, 811, DateTimeKind.Local).AddTicks(540));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 6, 22, 35, 17, 456, DateTimeKind.Local).AddTicks(3669),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 1, 2, 31, 26, 808, DateTimeKind.Local).AddTicks(1226));

            migrationBuilder.AddColumn<long>(
                name: "TelegramChatId",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramVerificationCode",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramChatId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TelegramVerificationCode",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 1, 2, 31, 26, 812, DateTimeKind.Local).AddTicks(3628),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 6, 22, 35, 17, 460, DateTimeKind.Local).AddTicks(5453));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "PaymentHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 1, 2, 31, 26, 811, DateTimeKind.Local).AddTicks(540),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 6, 22, 35, 17, 459, DateTimeKind.Local).AddTicks(2331));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 1, 2, 31, 26, 808, DateTimeKind.Local).AddTicks(1226),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 10, 6, 22, 35, 17, 456, DateTimeKind.Local).AddTicks(3669));
        }
    }
}
