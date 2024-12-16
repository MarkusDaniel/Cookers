using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cookers.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIngradientList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IngredientsString",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IngredientsString",
                table: "Recipes");
        }
    }
}
