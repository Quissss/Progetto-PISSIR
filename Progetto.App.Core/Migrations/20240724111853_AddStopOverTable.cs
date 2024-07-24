using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddStopOverTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 52, DateTimeKind.Local).AddTicks(8783),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 21, 20, 25, 22, 467, DateTimeKind.Local).AddTicks(3801));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 30, DateTimeKind.Local).AddTicks(9003),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 21, 20, 25, 22, 465, DateTimeKind.Local).AddTicks(6568));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 31, DateTimeKind.Local).AddTicks(47),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 21, 20, 25, 22, 465, DateTimeKind.Local).AddTicks(7284));

            migrationBuilder.CreateTable(
                name: "StopOver",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartStopOverTime = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 86, DateTimeKind.Local).AddTicks(4603)),
                    EndStopoverTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    CarPlate = table.Column<string>(type: "TEXT", nullable: true),
                    ParkingId = table.Column<int>(type: "INTEGER", nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(5, 2)", nullable: false),
                    ToPay = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StopOver", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StopOver_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StopOver_Cars_CarPlate",
                        column: x => x.CarPlate,
                        principalTable: "Cars",
                        principalColumn: "LicencePlate");
                    table.ForeignKey(
                        name: "FK_StopOver_Parkings_ParkingId",
                        column: x => x.ParkingId,
                        principalTable: "Parkings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StopOver_CarPlate",
                table: "StopOver",
                column: "CarPlate");

            migrationBuilder.CreateIndex(
                name: "IX_StopOver_ParkingId",
                table: "StopOver",
                column: "ParkingId");

            migrationBuilder.CreateIndex(
                name: "IX_StopOver_UserId",
                table: "StopOver",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StopOver");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "CurrentlyCharging",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 20, 25, 22, 467, DateTimeKind.Local).AddTicks(3801),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 52, DateTimeKind.Local).AddTicks(8783));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 20, 25, 22, 465, DateTimeKind.Local).AddTicks(6568),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 30, DateTimeKind.Local).AddTicks(9003));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndChargingTime",
                table: "ChargeHistory",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 21, 20, 25, 22, 465, DateTimeKind.Local).AddTicks(7284),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValue: new DateTime(2024, 7, 24, 13, 18, 49, 31, DateTimeKind.Local).AddTicks(47));
        }
    }
}
