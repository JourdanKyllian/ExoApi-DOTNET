using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Archi.API.Migrations
{
    /// <inheritdoc />
    public partial class LastUpdateMigr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Ingrédients",
                table: "Pizza",
                newName: "Ingredients");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Tacos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Tacos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Tacos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Tacos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Pizza",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Pizza",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Pizza",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Pizza",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Tacos");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Tacos");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Tacos");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Tacos");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Pizza");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Pizza");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Pizza");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Pizza");

            migrationBuilder.RenameColumn(
                name: "Ingredients",
                table: "Pizza",
                newName: "Ingrédients");
        }
    }
}
