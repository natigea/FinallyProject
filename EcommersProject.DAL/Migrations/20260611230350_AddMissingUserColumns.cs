using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommersProject.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingUserColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'EmailCodeExpiry' AND Object_ID = OBJECT_ID(N'Users'))
                    ALTER TABLE [Users] ADD [EmailCodeExpiry] DATETIMEOFFSET NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'EmailVerificationCode' AND Object_ID = OBJECT_ID(N'Users'))
                    ALTER TABLE [Users] ADD [EmailVerificationCode] NVARCHAR(10) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'FcmToken' AND Object_ID = OBJECT_ID(N'Users'))
                    ALTER TABLE [Users] ADD [FcmToken] NVARCHAR(500) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsEmailVerified' AND Object_ID = OBJECT_ID(N'Users'))
                    ALTER TABLE [Users] ADD [IsEmailVerified] BIT NOT NULL DEFAULT 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "EmailCodeExpiry", table: "Users");
            migrationBuilder.DropColumn(name: "EmailVerificationCode", table: "Users");
            migrationBuilder.DropColumn(name: "FcmToken", table: "Users");
            migrationBuilder.DropColumn(name: "IsEmailVerified", table: "Users");
        }
    }
}
