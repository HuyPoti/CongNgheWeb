using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class RecreateProductSpecsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductSpec_products_ProductId",
                table: "ProductSpec");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductSpec",
                table: "ProductSpec");

            migrationBuilder.RenameTable(
                name: "ProductSpec",
                newName: "product_specs");

            migrationBuilder.RenameColumn(
                name: "SpecValue",
                table: "product_specs",
                newName: "spec_value");

            migrationBuilder.RenameColumn(
                name: "SpecKey",
                table: "product_specs",
                newName: "spec_key");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "product_specs",
                newName: "product_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "product_specs",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ProductSpecId",
                table: "product_specs",
                newName: "spec_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProductSpec_ProductId",
                table: "product_specs",
                newName: "IX_product_specs_product_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_specs",
                table: "product_specs",
                column: "spec_id");

            migrationBuilder.AddForeignKey(
                name: "FK_product_specs_products_product_id",
                table: "product_specs",
                column: "product_id",
                principalTable: "products",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_specs_products_product_id",
                table: "product_specs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_specs",
                table: "product_specs");

            migrationBuilder.RenameTable(
                name: "product_specs",
                newName: "ProductSpec");

            migrationBuilder.RenameColumn(
                name: "spec_value",
                table: "ProductSpec",
                newName: "SpecValue");

            migrationBuilder.RenameColumn(
                name: "spec_key",
                table: "ProductSpec",
                newName: "SpecKey");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "ProductSpec",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ProductSpec",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "spec_id",
                table: "ProductSpec",
                newName: "ProductSpecId");

            migrationBuilder.RenameIndex(
                name: "IX_product_specs_product_id",
                table: "ProductSpec",
                newName: "IX_ProductSpec_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductSpec",
                table: "ProductSpec",
                column: "ProductSpecId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSpec_products_ProductId",
                table: "ProductSpec",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
