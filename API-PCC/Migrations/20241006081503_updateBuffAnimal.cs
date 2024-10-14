using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PCC.Migrations
{
    public partial class updateBuffAnimal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FarmerId",
                table: "A_Buff_Animal",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "A_Buff_Animal",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FarmerId",
                table: "A_Buff_Animal");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "A_Buff_Animal");
        }
    }
}
