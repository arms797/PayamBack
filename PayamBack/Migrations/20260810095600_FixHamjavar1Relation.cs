using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class FixHamjavar1Relation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hamjavar1s_Hamjavars_HamjavarId",
                table: "Hamjavar1s");

            migrationBuilder.DropForeignKey(
                name: "FK_Hamjavar1s_Hamjavars_HamjavarId1",
                table: "Hamjavar1s");

            migrationBuilder.DropIndex(
                name: "IX_Hamjavar1_HamjavarId",
                table: "Hamjavar1s");

            migrationBuilder.DropIndex(
                name: "IX_Hamjavar1s_HamjavarId1",
                table: "Hamjavar1s");

            migrationBuilder.DropColumn(
                name: "HamjavarId1",
                table: "Hamjavar1s");

            migrationBuilder.CreateIndex(
                name: "IX_Hamjavar1_HamjavarId",
                table: "Hamjavar1s",
                column: "HamjavarId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hamjavar1s_Hamjavars_HamjavarId",
                table: "Hamjavar1s",
                column: "HamjavarId",
                principalTable: "Hamjavars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hamjavar1s_Hamjavars_HamjavarId",
                table: "Hamjavar1s");

            migrationBuilder.DropIndex(
                name: "IX_Hamjavar1_HamjavarId",
                table: "Hamjavar1s");

            migrationBuilder.AddColumn<int>(
                name: "HamjavarId1",
                table: "Hamjavar1s",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hamjavar1_HamjavarId",
                table: "Hamjavar1s",
                column: "HamjavarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hamjavar1s_HamjavarId1",
                table: "Hamjavar1s",
                column: "HamjavarId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Hamjavar1s_Hamjavars_HamjavarId",
                table: "Hamjavar1s",
                column: "HamjavarId",
                principalTable: "Hamjavars",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Hamjavar1s_Hamjavars_HamjavarId1",
                table: "Hamjavar1s",
                column: "HamjavarId1",
                principalTable: "Hamjavars",
                principalColumn: "Id");
        }
    }
}
