using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PCC.Migrations
{
    public partial class FarmerAffilation2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AffiliationId",
                table: "Tbl_FarmerAffialiation",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AffiliationId",
                table: "Tbl_FarmerAffialiation",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
