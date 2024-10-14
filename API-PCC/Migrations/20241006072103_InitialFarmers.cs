using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PCC.Migrations
{
    public partial class InitialFarmers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
              name: "tblFarmers",
              columns: table => new
              {
                  Id = table.Column<int>(type: "int", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  TelephoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  User_Id = table.Column<int>(type: "int", nullable: false),
                  Group_Id = table.Column<int>(type: "int", nullable: false),
                  Is_Manager = table.Column<bool>(type: "bit", nullable: false),
                  FarmerClassification_Id = table.Column<int>(type: "int", nullable: false),
                  FarmerAffliation_Id = table.Column<int>(type: "int", nullable: false),
                  Created_By = table.Column<int>(type: "int", nullable: false),
                  Created_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                  Updated_By = table.Column<int>(type: "int", nullable: false),
                  Updated_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                  Deleted_By = table.Column<int>(type: "int", nullable: false),
                  Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  Deleted_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                  Is_Deleted = table.Column<bool>(type: "bit", nullable: false)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_tblFarmers", x => x.Id);
              });
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "tblFarmers");
        }
        }
}
