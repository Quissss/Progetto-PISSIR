using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddOnDelete_ImmReqOnCurrCharg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentlyCharging_ImmediateRequests_ImmediateRequestId",
                table: "CurrentlyCharging");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 25, 23, 21, 33, 192, DateTimeKind.Local).AddTicks(137),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 25, 20, 44, 55, 677, DateTimeKind.Local).AddTicks(9232));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 25, 23, 21, 33, 188, DateTimeKind.Local).AddTicks(6247),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 25, 20, 44, 55, 674, DateTimeKind.Local).AddTicks(3622));

            migrationBuilder.AlterColumn<int>(
                name: "ImmediateRequestId",
                table: "CurrentlyCharging",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 25, 23, 21, 33, 187, DateTimeKind.Local).AddTicks(6341),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 25, 20, 44, 55, 673, DateTimeKind.Local).AddTicks(3557));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 25, 23, 21, 33, 187, DateTimeKind.Local).AddTicks(6858),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 25, 20, 44, 55, 673, DateTimeKind.Local).AddTicks(4077));

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentlyCharging_ImmediateRequests_ImmediateRequestId",
                table: "CurrentlyCharging",
                column: "ImmediateRequestId",
                principalTable: "ImmediateRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentlyCharging_ImmediateRequests_ImmediateRequestId",
                table: "CurrentlyCharging");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 25, 20, 44, 55, 677, DateTimeKind.Local).AddTicks(9232),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 25, 23, 21, 33, 192, DateTimeKind.Local).AddTicks(137));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 25, 20, 44, 55, 674, DateTimeKind.Local).AddTicks(3622),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 25, 23, 21, 33, 188, DateTimeKind.Local).AddTicks(6247));

            migrationBuilder.AlterColumn<int>(
                name: "ImmediateRequestId",
                table: "CurrentlyCharging",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 25, 20, 44, 55, 673, DateTimeKind.Local).AddTicks(3557),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 25, 23, 21, 33, 187, DateTimeKind.Local).AddTicks(6341));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 25, 20, 44, 55, 673, DateTimeKind.Local).AddTicks(4077),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 25, 23, 21, 33, 187, DateTimeKind.Local).AddTicks(6858));

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentlyCharging_ImmediateRequests_ImmediateRequestId",
                table: "CurrentlyCharging",
                column: "ImmediateRequestId",
                principalTable: "ImmediateRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
