// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AdminMessageDismiss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "dismissed",
                table: "admin_messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE admin_messages SET dismissed = seen;");

            migrationBuilder.AddCheckConstraint(
                name: "NotDismissedAndSeen",
                table: "admin_messages",
                sql: "NOT dismissed OR seen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "NotDismissedAndSeen",
                table: "admin_messages");

            migrationBuilder.DropColumn(
                name: "dismissed",
                table: "admin_messages");
        }
    }
}
