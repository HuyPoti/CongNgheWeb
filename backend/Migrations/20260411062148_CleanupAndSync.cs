using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class CleanupAndSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:banner_position", "category_top,homepage_mid,homepage_slider,news_top")
                .OldAnnotation("Npgsql:Enum:banner_position.banner_position", "homepage_slider,homepage_mid,category_top,news_top");

            migrationBuilder.AlterColumn<string>(
                name: "specifications",
                table: "products",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "position",
                table: "banners",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "banner_position");

            migrationBuilder.AlterColumn<string>(
                name: "link_url",
                table: "banners",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "image_url",
                table: "banners",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:banner_position", "category_top,homepage_mid,homepage_slider,news_top")
                .Annotation("Npgsql:Enum:banner_position.banner_position", "homepage_slider,homepage_mid,category_top,news_top");

            migrationBuilder.AlterColumn<string>(
                name: "specifications",
                table: "products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "position",
                table: "banners",
                type: "banner_position",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "link_url",
                table: "banners",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "image_url",
                table: "banners",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
