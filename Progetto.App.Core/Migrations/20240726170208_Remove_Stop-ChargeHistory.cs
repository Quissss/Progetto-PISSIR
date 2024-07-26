using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class Remove_StopChargeHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChargeHistory");

            migrationBuilder.DropTable(
                name: "StopoverHistory");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 26, 19, 2, 7, 33, DateTimeKind.Local).AddTicks(9487),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 26, 13, 33, 18, 291, DateTimeKind.Local).AddTicks(2209));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "PaymentHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 26, 19, 2, 7, 32, DateTimeKind.Local).AddTicks(5011),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 26, 13, 33, 18, 289, DateTimeKind.Local).AddTicks(5270));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 26, 19, 2, 7, 29, DateTimeKind.Local).AddTicks(3022),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 26, 13, 33, 18, 285, DateTimeKind.Local).AddTicks(8888));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartStopoverTime",
                table: "Stopover",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 26, 13, 33, 18, 291, DateTimeKind.Local).AddTicks(2209),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 26, 19, 2, 7, 33, DateTimeKind.Local).AddTicks(9487));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "PaymentHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 26, 13, 33, 18, 289, DateTimeKind.Local).AddTicks(5270),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 26, 19, 2, 7, 32, DateTimeKind.Local).AddTicks(5011));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 26, 13, 33, 18, 285, DateTimeKind.Local).AddTicks(8888),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 26, 19, 2, 7, 29, DateTimeKind.Local).AddTicks(3022));

            migrationBuilder.CreateTable(
                name: "ChargeHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarPlate = table.Column<string>(type: "TEXT", nullable: true),
                    MwBotId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParkingSlotId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    EndChargingTime = table.Column<DateTime>(type: "datetime", nullable: false, defaultValue: new DateTime(2024, 7, 26, 13, 33, 18, 284, DateTimeKind.Local).AddTicks(728)),
                    EnergyConsumed = table.Column<decimal>(type: "decimal(5, 2)", nullable: false, defaultValue: 0m),
                    StartChargePercentage = table.Column<decimal>(type: "decimal(5, 2)", nullable: true),
                    StartChargingTime = table.Column<DateTime>(type: "datetime", nullable: false, defaultValue: new DateTime(2024, 7, 26, 13, 33, 18, 284, DateTimeKind.Local).AddTicks(110)),
                    TargetChargePercentage = table.Column<decimal>(type: "decimal(5, 2)", nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(5, 2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargeHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargeHistory_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChargeHistory_Cars_CarPlate",
                        column: x => x.CarPlate,
                        principalTable: "Cars",
                        principalColumn: "LicencePlate");
                    table.ForeignKey(
                        name: "FK_ChargeHistory_MWBots_MwBotId",
                        column: x => x.MwBotId,
                        principalTable: "MWBots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChargeHistory_ParkingSlots_ParkingSlotId",
                        column: x => x.ParkingSlotId,
                        principalTable: "ParkingSlots",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StopoverHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarPlate = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    EndStopoverTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartStopoverTime = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(2024, 7, 26, 13, 33, 18, 292, DateTimeKind.Local).AddTicks(1864)),
                    TotalCost = table.Column<decimal>(type: "decimal(5, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StopoverHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StopoverHistory_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StopoverHistory_Cars_CarPlate",
                        column: x => x.CarPlate,
                        principalTable: "Cars",
                        principalColumn: "LicencePlate",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChargeHistory_CarPlate",
                table: "ChargeHistory",
                column: "CarPlate");

            migrationBuilder.CreateIndex(
                name: "IX_ChargeHistory_MwBotId",
                table: "ChargeHistory",
                column: "MwBotId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargeHistory_ParkingSlotId",
                table: "ChargeHistory",
                column: "ParkingSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargeHistory_UserId",
                table: "ChargeHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StopoverHistory_CarPlate",
                table: "StopoverHistory",
                column: "CarPlate");

            migrationBuilder.CreateIndex(
                name: "IX_StopoverHistory_UserId",
                table: "StopoverHistory",
                column: "UserId");
        }
    }
}
