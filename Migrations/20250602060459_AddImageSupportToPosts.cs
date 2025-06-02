using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyASPProject.Migrations
{
    /// <inheritdoc />
    public partial class AddImageSupportToPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "imagealttext",
                table: "posts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "imagecontenttype",
                table: "posts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "imagefilename",
                table: "posts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "imagefilesize",
                table: "posts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "imageurl",
                table: "posts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imagealttext",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "imagecontenttype",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "imagefilename",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "imagefilesize",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "imageurl",
                table: "posts");
        }
    }
}
