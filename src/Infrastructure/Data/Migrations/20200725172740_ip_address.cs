using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.Nnn.Infrastructure.Data.Migrations
{
    public partial class ip_address : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Tags",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Suggestions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "ReplyLikes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Reply",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "PostVotes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "PostTags",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Posts",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "PostCategories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "ModeratorOperations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Messages",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Conversations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "CommunityUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Communities",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Comments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "CommentLikes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Category",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Suggestions");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "ReplyLikes");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Reply");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "PostVotes");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "PostTags");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "PostCategories");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "ModeratorOperations");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "CommunityUsers");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Communities");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "CommentLikes");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Category");
        }
    }
}
