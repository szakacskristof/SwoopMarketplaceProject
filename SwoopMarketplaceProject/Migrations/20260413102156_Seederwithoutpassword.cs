using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwoopMarketplaceProject.Migrations
{
    /// <inheritdoc />
    public partial class Seederwithoutpassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "condition",
                table: "listings",
                type: "enum('Új','Kiváló','Kielégítő','Használt','Hibás')",
                nullable: false,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "enum('fn','mw','ft','ww','bs')")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "condition",
                table: "listings",
                type: "enum('fn','mw','ft','ww','bs')",
                nullable: false,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "enum('Új','Kiváló','Kielégítő','Használt','Hibás')")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");
        }
    }
}
