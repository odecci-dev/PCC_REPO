using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PCC.Migrations
{
    public partial class updateBuffAnimalFarmerId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FarmerId",
                table: "H_Buff_Herd",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FarmerId",
                table: "H_Buff_Herd");
        }
    }
}
