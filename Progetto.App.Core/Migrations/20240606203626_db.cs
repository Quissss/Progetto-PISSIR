using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progetto.App.Core.Migrations
{
    /// <inheritdoc />
    public partial class db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSlot_Parkings_Id",
                table: "ParkingSlot");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_AspNetUsers_UserId",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_ParkingSlot_ParkingSlotId",
                table: "Reservation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reservation",
                table: "Reservation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkingSlot",
                table: "ParkingSlot");

            migrationBuilder.RenameTable(
                name: "Reservation",
                newName: "Reservations");

            migrationBuilder.RenameTable(
                name: "ParkingSlot",
                newName: "ParkingSlots");

            migrationBuilder.RenameIndex(
                name: "IX_Reservation_UserId",
                table: "Reservations",
                newName: "IX_Reservations_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reservation_ParkingSlotId",
                table: "Reservations",
                newName: "IX_Reservations_ParkingSlotId");

            migrationBuilder.AddColumn<int>(
                name: "ParkingSlotId",
                table: "Cars",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "RequestedChargeLevel",
                table: "Reservations",
                type: "TEXT",
                nullable: false,
                defaultValue: 100m,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ParkingSlots",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "ParkingSlots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParkingId",
                table: "ParkingSlots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reservations",
                table: "Reservations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingSlots",
                table: "ParkingSlots",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_ParkingSlotId",
                table: "Cars",
                column: "ParkingSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSlots_ParkingId",
                table: "ParkingSlots",
                column: "ParkingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cars_ParkingSlots_ParkingSlotId",
                table: "Cars",
                column: "ParkingSlotId",
                principalTable: "ParkingSlots",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSlots_Parkings_ParkingId",
                table: "ParkingSlots",
                column: "ParkingId",
                principalTable: "Parkings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_UserId",
                table: "Reservations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ParkingSlots_ParkingSlotId",
                table: "Reservations",
                column: "ParkingSlotId",
                principalTable: "ParkingSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cars_ParkingSlots_ParkingSlotId",
                table: "Cars");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSlots_Parkings_ParkingId",
                table: "ParkingSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_UserId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ParkingSlots_ParkingSlotId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Cars_ParkingSlotId",
                table: "Cars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reservations",
                table: "Reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkingSlots",
                table: "ParkingSlots");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSlots_ParkingId",
                table: "ParkingSlots");

            migrationBuilder.DropColumn(
                name: "ParkingSlotId",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "ParkingSlots");

            migrationBuilder.DropColumn(
                name: "ParkingId",
                table: "ParkingSlots");

            migrationBuilder.RenameTable(
                name: "Reservations",
                newName: "Reservation");

            migrationBuilder.RenameTable(
                name: "ParkingSlots",
                newName: "ParkingSlot");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_UserId",
                table: "Reservation",
                newName: "IX_Reservation_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_ParkingSlotId",
                table: "Reservation",
                newName: "IX_Reservation_ParkingSlotId");

            migrationBuilder.AlterColumn<decimal>(
                name: "RequestedChargeLevel",
                table: "Reservation",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldDefaultValue: 100m);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ParkingSlot",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reservation",
                table: "Reservation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingSlot",
                table: "ParkingSlot",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSlot_Parkings_Id",
                table: "ParkingSlot",
                column: "Id",
                principalTable: "Parkings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_AspNetUsers_UserId",
                table: "Reservation",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_ParkingSlot_ParkingSlotId",
                table: "Reservation",
                column: "ParkingSlotId",
                principalTable: "ParkingSlot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
