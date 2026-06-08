using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommersProject.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Columns already exist in DB (added via raw SQL in Program.cs).
            // This migration only syncs the EF model snapshot.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
