using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addphonecol : Migration
    {
        private readonly string sp_getAppointments = @"
            create procedure Appointments_getData
                as begin
                select * from Appointments
                 end
            ";
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "phoneno",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.Sql(
                sp_getAppointments
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "phoneno",
                table: "Appointments");
            migrationBuilder.Sql(
               "drop procedure Appointments_getData");
        }
    }
}
