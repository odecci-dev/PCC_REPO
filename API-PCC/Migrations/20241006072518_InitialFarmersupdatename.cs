using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PCC.Migrations
{
    public partial class InitialFarmersupdatename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_tblFarmers",
                table: "tblFarmers");

            migrationBuilder.RenameTable(
                name: "tblFarmers",
                newName: "Tbl_Farmers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tbl_Farmers",
                table: "Tbl_Farmers",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Tbl_Farmers",
                table: "Tbl_Farmers");

            migrationBuilder.RenameTable(
                name: "Tbl_Farmers",
                newName: "tblFarmers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tblFarmers",
                table: "tblFarmers",
                column: "Id");
        }
    }
}
