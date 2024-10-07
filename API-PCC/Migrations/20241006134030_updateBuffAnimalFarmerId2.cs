using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PCC.Migrations
{
    public partial class updateBuffAnimalFarmerId2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BloodComp",
                table: "A_Buff_Animal",
                newName: "Blood_Comp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Blood_Comp",
                table: "A_Buff_Animal",
                newName: "BloodComp");
        }
    }
}
