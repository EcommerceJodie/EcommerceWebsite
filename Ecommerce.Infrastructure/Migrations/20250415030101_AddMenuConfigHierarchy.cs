using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Infrastructure.Migrations
{

    public partial class AddMenuConfigHierarchy : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "MenuConfigs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuConfigs_ParentId",
                table: "MenuConfigs",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuConfigs_MenuConfigs_ParentId",
                table: "MenuConfigs",
                column: "ParentId",
                principalTable: "MenuConfigs",
                principalColumn: "Id");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuConfigs_MenuConfigs_ParentId",
                table: "MenuConfigs");

            migrationBuilder.DropIndex(
                name: "IX_MenuConfigs_ParentId",
                table: "MenuConfigs");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "MenuConfigs");
        }
    }
}
