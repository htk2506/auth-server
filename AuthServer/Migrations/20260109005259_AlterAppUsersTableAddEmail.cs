using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthServer.Migrations
{
    /// <inheritdoc />
    public partial class AlterAppUsersTableAddEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "app_users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_app_users_email",
                table: "app_users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_app_users_email",
                table: "app_users");

            migrationBuilder.DropColumn(
                name: "email",
                table: "app_users");
        }
    }
}
