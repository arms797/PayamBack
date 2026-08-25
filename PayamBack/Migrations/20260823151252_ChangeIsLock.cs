using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayamBack.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIsLock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BarnamehHaftegiOstad1_BarnamehHaftegiOstads_BarnamehHaftegiOstadId",
                table: "BarnamehHaftegiOstad1");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BarnamehHaftegiOstad1",
                table: "BarnamehHaftegiOstad1");

            migrationBuilder.RenameTable(
                name: "BarnamehHaftegiOstad1",
                newName: "BarnamehHaftegiOstad1s");

            migrationBuilder.RenameColumn(
                name: "NararElmi",
                table: "BarnamehHaftegiOstads",
                newName: "NazarElmi");

            migrationBuilder.RenameColumn(
                name: "IsLock",
                table: "BarnamehHaftegiOstads",
                newName: "IsLocked");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BarnamehHaftegiOstad1s",
                table: "BarnamehHaftegiOstad1s",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BarnamehHaftegiOstad1s_BarnamehHaftegiOstads_BarnamehHaftegiOstadId",
                table: "BarnamehHaftegiOstad1s",
                column: "BarnamehHaftegiOstadId",
                principalTable: "BarnamehHaftegiOstads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BarnamehHaftegiOstad1s_BarnamehHaftegiOstads_BarnamehHaftegiOstadId",
                table: "BarnamehHaftegiOstad1s");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BarnamehHaftegiOstad1s",
                table: "BarnamehHaftegiOstad1s");

            migrationBuilder.RenameTable(
                name: "BarnamehHaftegiOstad1s",
                newName: "BarnamehHaftegiOstad1");

            migrationBuilder.RenameColumn(
                name: "NazarElmi",
                table: "BarnamehHaftegiOstads",
                newName: "NararElmi");

            migrationBuilder.RenameColumn(
                name: "IsLocked",
                table: "BarnamehHaftegiOstads",
                newName: "IsLock");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BarnamehHaftegiOstad1",
                table: "BarnamehHaftegiOstad1",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BarnamehHaftegiOstad1_BarnamehHaftegiOstads_BarnamehHaftegiOstadId",
                table: "BarnamehHaftegiOstad1",
                column: "BarnamehHaftegiOstadId",
                principalTable: "BarnamehHaftegiOstads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
