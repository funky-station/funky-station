// SPDX-FileCopyrightText: 2026 AftrLite <61218133+AftrLite@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later


using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server._DV.CosmicCult.Abilities;
using Content.Server.Administration;
using Content.Server.Antag;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Console;
using Robust.Shared.Player;

[AdminCommand(AdminFlags.Fun)]
public sealed class CultifyBorgCommand : LocalizedEntityCommands
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly ISharedPlayerManager _players = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    public override string Command => "cultifyborg";
    public override string Description => "Imprisons a given borg in a chantry.";
    public override string Help => "Usage: cultifyborg <entity uid>";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { AttachedEntity: { } entity })
            return;

        if (args.Length is < 1 or > 1)
        {
            shell.WriteError("Incorrect number of arguments. " + Help);
            return;
        }

        if (!TryGetVictimFromUidOrUsername(args[0], shell, out var victim))
        {
            shell.WriteError(Loc.GetString("cmd-failure-no-attached-entity"));
            return;
        }

        if (!_mind.TryGetMind(victim.Value, out var mindId, out var mind))
        {
            shell.WriteError(Loc.GetString("cmd-cultify-failure-no-entity"));
            return;
        }

        if (_entities.GetComponent<BorgChassisComponent>(victim.Value) is null)
        {
            shell.WriteError(Loc.GetString("cmd-cultify-failure-not-borg"));
            return;
        }

        var entityCoordinates = _entities.GetComponent<TransformComponent>(victim.Value).Coordinates;
        var wisp = _entities.SpawnEntity("CosmicChantryWisp", entityCoordinates);
        var chantry = _entities.SpawnEntity("CosmicBorgChantry", entityCoordinates);
        _entities.EnsureComponent<CosmicChantryComponent>(chantry, out var chantryComponent);
        chantryComponent.InternalVictim = wisp;
        chantryComponent.VictimBody = victim.Value;
        _mind.TransferTo(mindId, wisp, mind: mind);

        var mins = chantryComponent.EventTime.Minutes;
        var secs = chantryComponent.EventTime.Seconds;
        _antag.SendBriefing(wisp, Loc.GetString("cosmiccult-silicon-chantry-briefing", ("minutesandseconds", $"{mins} minutes and {secs} seconds")), Color.FromHex("#4cabb3"), null);
    }

    private bool TryGetVictimFromUidOrUsername(
        string str,
        IConsoleShell shell,
        [NotNullWhen(true)] out EntityUid? victimUid)
    {
        if (NetEntity.TryParse(str, out var uidNet) && _entities.TryGetEntity(uidNet, out var uid))
        {
            victimUid = uid;
            return true;
        }
        else if (_players.TryGetSessionByUsername(str, out var session) && session.AttachedEntity != null)
        {
            victimUid = session.AttachedEntity;
            return true;
        }

        shell.WriteError(Loc.GetString("cmd-tpto-parse-error", ("str", str)));

        victimUid = default;
        return false;
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 0)
            return CompletionResult.Empty;

        var last = args[^1];

        var users = _players.Sessions
            .Select(x => x.Name ?? string.Empty)
            .Where(x => !string.IsNullOrWhiteSpace(x) && x.StartsWith(last, System.StringComparison.CurrentCultureIgnoreCase));

        var hint = "cmd-cultify-borg-hint";
        hint = Loc.GetString(hint);
        return CompletionResult.FromHintOptions(users, hint);
    }

}
