using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class EnergyColoumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChargeHistory_MWBots_MWBotId",
                table: "ChargeHistory");

            migrationBuilder.RenameColumn(
                name: "EnergyCostPerMinute",
                table: "Parkings",
                newName: "StopCostPerMinute");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "ChargeHistory",
                newName: "ParkStartDate");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "ChargeHistory",
                newName: "ParkEndDate");

            migrationBuilder.AddColumn<decimal>(
                name: "EnergyCostPerKw",
                table: "Parkings",
                type: "decimal(5, 2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "MWBotId",
                table: "ChargeHistory",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "CarId",
                table: "ChargeHistory",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CarLicencePlate",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ChargeEndDate",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ChargeStartDate",
                table: "ChargeHistory",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "BatteryPercentage",
                table: "Cars",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsElectric",
                table: "Cars",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Cars",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ChargeHistory_CarLicencePlate",
                table: "ChargeHistory",
                column: "CarLicencePlate");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargeHistory_Cars_CarLicencePlate",
                table: "ChargeHistory",
                column: "CarLicencePlate",
                principalTable: "Cars",
                principalColumn: "LicencePlate",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChargeHistory_MWBots_MWBotId",
                table: "ChargeHistory",
                column: "MWBotId",
                principalTable: "MWBots",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChargeHistory_Cars_CarLicencePlate",
                table: "ChargeHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_ChargeHistory_MWBots_MWBotId",
                table: "ChargeHistory");

            migrationBuilder.DropIndex(
                name: "IX_ChargeHistory_CarLicencePlate",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "EnergyCostPerKw",
                table: "Parkings");

            migrationBuilder.DropColumn(
                name: "CarId",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "CarLicencePlate",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "ChargeEndDate",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "ChargeStartDate",
                table: "ChargeHistory");

            migrationBuilder.DropColumn(
                name: "BatteryPercentage",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "IsElectric",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Cars");

            migrationBuilder.RenameColumn(
                name: "StopCostPerMinute",
                table: "Parkings",
                newName: "EnergyCostPerMinute");

            migrationBuilder.RenameColumn(
                name: "ParkStartDate",
                table: "ChargeHistory",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "ParkEndDate",
                table: "ChargeHistory",
                newName: "EndDate");

            migrationBuilder.AlterColumn<int>(
                name: "MWBotId",
                table: "ChargeHistory",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChargeHistory_MWBots_MWBotId",
                table: "ChargeHistory",
                column: "MWBotId",
                principalTable: "MWBots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
