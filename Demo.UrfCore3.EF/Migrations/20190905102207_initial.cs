using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Demo.UrfCore3.EF.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProductName = table.Column<string>(nullable: true),
                    UnitPrice = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "ProductName", "UnitPrice" },
                values: new object[] { 1, "Chai", 1m });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "ProductName", "UnitPrice" },
                values: new object[] { 2, "Chang", 2m });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "ProductName", "UnitPrice" },
                values: new object[] { 3, "Cappuccino", 3m });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
