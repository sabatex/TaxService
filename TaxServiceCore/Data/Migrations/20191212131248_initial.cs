using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TaxService.Data.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PlatformType = table.Column<int>(nullable: false),
                    ConfigType = table.Column<int>(nullable: false),
                    ServerLocation = table.Column<int>(nullable: false),
                    ServerAdress = table.Column<string>(nullable: true),
                    DataBaseName = table.Column<string>(nullable: true),
                    DataBasePath = table.Column<string>(nullable: true),
                    UserName = table.Column<string>(nullable: true),
                    UserPass = table.Column<string>(nullable: true),
                    UseLocalKey = table.Column<bool>(nullable: false),
                    Exclusive = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    FullName = table.Column<string>(nullable: true),
                    IS_NP = table.Column<bool>(nullable: false),
                    C_REG = table.Column<int>(nullable: false),
                    C_RAJ = table.Column<int>(nullable: false),
                    FilialNumber = table.Column<string>(nullable: true),
                    Manager = table.Column<string>(nullable: true),
                    ManagerIPN = table.Column<string>(nullable: true),
                    CODE_IN_1C = table.Column<string>(nullable: true),
                    J12010_OutPath = table.Column<string>(nullable: true),
                    J12010_StartNumber = table.Column<int>(nullable: false),
                    AutoPDV = table.Column<bool>(nullable: false),
                    IsReadOnly = table.Column<bool>(nullable: false),
                    FirmCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
