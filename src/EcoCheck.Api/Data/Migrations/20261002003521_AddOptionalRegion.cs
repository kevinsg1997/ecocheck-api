using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoCheck.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOptionalRegion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "country_code",
                table: "survey_responses",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state_code",
                table: "survey_responses",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_survey_responses_country_code_state_code",
                table: "survey_responses",
                columns: new[] { "country_code", "state_code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_survey_responses_country_code_state_code",
                table: "survey_responses");

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "survey_responses");

            migrationBuilder.DropColumn(
                name: "state_code",
                table: "survey_responses");
        }
    }
}
