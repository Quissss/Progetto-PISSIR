using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestIdOnCurrentlyCharging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 20, 25, 22, 467, DateTimeKind.Local).AddTicks(3801),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 21, 18, 4, 55, 437, DateTimeKind.Local).AddTicks(9428));

            migrationBuilder.AddColumn<int>(
                name: "ImmediateRequestId",
                table: "CurrentlyCharging",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 20, 25, 22, 465, DateTimeKind.Local).AddTicks(6568),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 21, 18, 4, 55, 436, DateTimeKind.Local).AddTicks(9456));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 20, 25, 22, 465, DateTimeKind.Local).AddTicks(7284),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 21, 18, 4, 55, 436, DateTimeKind.Local).AddTicks(9970));

            migrationBuilder.CreateIndex(
                name: "IX_CurrentlyCharging_ImmediateRequestId",
                table: "CurrentlyCharging",
                column: "ImmediateRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentlyCharging_ImmediateRequests_ImmediateRequestId",
                table: "CurrentlyCharging",
                column: "ImmediateRequestId",
                principalTable: "ImmediateRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentlyCharging_ImmediateRequests_ImmediateRequestId",
                table: "CurrentlyCharging");

            migrationBuilder.DropIndex(
                name: "IX_CurrentlyCharging_ImmediateRequestId",
                table: "CurrentlyCharging");

            migrationBuilder.DropColumn(
                name: "ImmediateRequestId",
                table: "CurrentlyCharging");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 18, 4, 55, 437, DateTimeKind.Local).AddTicks(9428),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 21, 20, 25, 22, 467, DateTimeKind.Local).AddTicks(3801));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 18, 4, 55, 436, DateTimeKind.Local).AddTicks(9456),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 21, 20, 25, 22, 465, DateTimeKind.Local).AddTicks(6568));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 18, 4, 55, 436, DateTimeKind.Local).AddTicks(9970),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 21, 20, 25, 22, 465, DateTimeKind.Local).AddTicks(7284));
        }
    }
}
