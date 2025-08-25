// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Content.Server.Database;
using Robust.Server.ServerStatus;

namespace Content.Server.Administration;

public sealed partial class ServerApi
{
    [Dependency] private readonly IServerDbManager _dbManager = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;

    public void InitializeFunky()
    {
        RegisterHandler(HttpMethod.Post, "/admin/actions/whitelist", ActionWhitelist);
    }

    /// <summary>
    ///     Whitelists a player.
    /// </summary>
    private async Task ActionWhitelist(IStatusHandlerContext context)
    {
        var body = await ReadJson<WhitelistActionBody>(context);

        if (body == null)
            return;

        var data = await _playerLocator.LookupIdByNameOrIdAsync(body.Username);

        if (data == null)
        {
            await RespondError(
                context,
                ErrorCode.PlayerNotFound,
                HttpStatusCode.UnprocessableContent,
                "Player not found");
            return;
        }

        var isWhitelisted = await _dbManager.GetWhitelistStatusAsync(data.UserId);
        var whitelisting = body.IsAddingWhitelist is true or null;

        if (isWhitelisted && whitelisting)
        {
            await RespondError(
                context,
                ErrorCode.BadRequest,
                HttpStatusCode.Conflict,
                "Already whitelisted");
            return;
        }

        if (!isWhitelisted && !whitelisting)
        {
            await RespondError(
                context,
                ErrorCode.BadRequest,
                HttpStatusCode.NotFound,
                "Not whitelisted");
            return;
        }

        if (whitelisting)
            await _dbManager.AddToWhitelistAsync(data.UserId);
        else
            await _dbManager.RemoveFromWhitelistAsync(data.UserId);

        await RespondOk(context);
    }

    private sealed class WhitelistActionBody
    {
        public required string Username { get; init; }
        public bool? IsAddingWhitelist { get; init; }
    }
}
