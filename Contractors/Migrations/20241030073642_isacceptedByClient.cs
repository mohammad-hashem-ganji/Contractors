using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contractors.Migrations
{
    /// <inheritdoc />
    public partial class isacceptedByClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsTenderOver",
                table: "Requests",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsFileCheckedByClient",
                table: "Requests",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Requests",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAcceptedByClient",
                table: "Requests",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5c380eb4-b07a-451e-a833-be5951a0526d", "AQAAAAIAAYagAAAAEOVy86/lcrxokMAdxVTigICGWyoDCYK5eAc34fpBEc/nYj2Csfb3fGQyPArAQSJgXQ==", "adda5add-872d-4726-93ff-f1766ffc1aa1" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsTenderOver",
                table: "Requests",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsFileCheckedByClient",
                table: "Requests",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Requests",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsAcceptedByClient",
                table: "Requests",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c6c75283-53a3-4ac5-8966-6dbd272efafc", "AQAAAAIAAYagAAAAEJ5JFyI/Qn9gdyIEMRxKPd0IEiCMPLRQ1LtbpxMTl1AZ5Gsw29icpHG8qKWskElM7A==", "e83a9917-01dc-4c9b-9c63-30cc10919ac4" });
        }
    }
}
