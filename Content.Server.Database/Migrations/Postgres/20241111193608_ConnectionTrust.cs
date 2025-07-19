// SPDX-FileCopyrightText: 2020 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class ConnectionTrust : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "trust",
                table: "connection_log",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "trust",
                table: "connection_log");
        }
    }
}
