using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zynapse.Backend.Persistence.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddCartUserIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_carts_UserId",
                schema: "public",
                table: "carts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_carts_UserId",
                schema: "public",
                table: "carts");
        }
    }
}
