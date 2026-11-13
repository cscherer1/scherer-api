using System.Text.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scherer.Api.Data.Migrations
{
    /// <summary>
    /// Replaces the early Burns & McDonnell project copy with the reviewed
    /// version focused on enterprise and M365 process automation through
    /// ServiceNow, Azure and PowerShell. Data-only; no schema change.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261113233000_CorrectBurnsAutomationCopy")]
    public partial class CorrectBurnsAutomationCopy : Migration
    {
        private static string Q(string s) => "'" + s.Replace("'", "''") + "'";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var tech = Q(JsonSerializer.Serialize(new[]
            {
                "Azure", "ServiceNow", "PowerShell", "Microsoft 365", "EfficientIP", "ArcGIS", "REST / OAuth"
            }));

            migrationBuilder.Sql($"""
                UPDATE "Projects" SET
                    "Blurb" = {Q("Automate enterprise and M365 processes through ServiceNow, Azure and PowerShell: access and licensing changes, IP-address management, backup and restore, and supporting data workflows. Examples include EfficientIP pool reservations, Revit server and ImaginIT Clarity automation, delegated manager access to departed employees' files, and ArcGIS backup and restore in Azure. The work removed hundreds of administrative hours a year and replaced hand-run processes with documented, supported services.")},
                    "Role"  = {Q("Staff Software Developer · Burns & McDonnell")},
                    "Tech"  = {tech}
                WHERE "Id" = 'enterprise-integration-automation';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data-only correction. Reverting would restore inaccurate copy.
        }
    }
}
