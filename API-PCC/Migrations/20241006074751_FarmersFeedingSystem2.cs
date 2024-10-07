using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PCC.Migrations
{
    public partial class FarmersFeedingSystem2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Farner_Id",
                table: "tbl_FarmerFeedingSystem",
                newName: "Farmer_Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Farmer_Id",
                table: "tbl_FarmerFeedingSystem",
                newName: "Farner_Id");
        }
    }
}
