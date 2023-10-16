using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HermesChat.Data.Migrations
{
    public partial class UpdatedUserIdDataType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a2981cbd-1961-4d96-a9d1-ee1a11b153aa");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b9c5adc7-1f0a-45a3-a6f0-d29a815cb650");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Profile",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Message",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ChatUser",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "8e7e9f84-727a-45a9-b3e3-589822d70d2d", "075c95e5-06e6-494f-b4b3-8a65e5479773", "Moderator", "MODERATOR" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "d4563b6b-3f9e-46b3-8e2c-f17db6ce8b7d", "17f4bbbb-2507-44ab-8898-7b8dd8afed8a", "User", "USER" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8e7e9f84-727a-45a9-b3e3-589822d70d2d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d4563b6b-3f9e-46b3-8e2c-f17db6ce8b7d");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Profile",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Message",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "ChatUser",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "a2981cbd-1961-4d96-a9d1-ee1a11b153aa", "a1fd58fd-7edc-46cf-8ff3-2bafa16ca42e", "User", "USER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "b9c5adc7-1f0a-45a3-a6f0-d29a815cb650", "d61b1a11-2d94-4b23-ab67-37e579d3b62c", "Moderator", "MODERATOR" });
        }
    }
}
