// SPDX-FileCopyrightText: 2023 LankLTE <135308300+LankLTE@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 BIGZi0348 <118811750+BIGZi0348@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mephisto72 <66994453+Mephisto72@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 jackel234 <jackel234@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 maelines <genovedd.almn@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

// using Content.Server.Silicons.Laws; imp remove
using Content.Server._CD.Traits; // imp
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Station.Components;
using Robust.Shared.Random; // imp
// CD - start synth trait
using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Robust.Shared.Player;
using Robust.Shared.Random;
// CD - end synth trait

namespace Content.Server.StationEvents.Events;

public sealed class IonStormRule : StationEventSystem<IonStormRuleComponent>
{
    // [Dependency] private readonly IonStormSystem _ionStorm = default!; // imp remove
    [Dependency] private readonly IRobustRandom _random = default!; // imp
    [Dependency] private readonly IChatManager _chatManager = default!; // CD - Used for synth trait

    protected override void Started(EntityUid uid, IonStormRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation))
            return;

         // CD - Go through everyone with the SynthComponent and inform them a storm is happening.
        var synthQuery = EntityQueryEnumerator<SynthComponent>();
        while (synthQuery.MoveNext(out var ent, out var synthComp))
        {
            if (RobustRandom.Prob(synthComp.AlertChance))
                continue;

            if (!TryComp<ActorComponent>(ent, out var actor))
                continue;

            var msg = Loc.GetString("station-event-ion-storm-synth");
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
            _chatManager.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.Yellow);
        }
        // CD - End of synth trait

        // begin imp edit, why tf wasnt this all just an event
        // var query = EntityQueryEnumerator<SiliconLawBoundComponent, TransformComponent, IonStormTargetComponent>();
        var query = EntityQueryEnumerator<IonStormTargetComponent, TransformComponent>();
        while (query.MoveNext(out var ent, out var target, out var xform))
        // end imp edit
        {
            // only affect law holders on the station, and check random chance (imp edit)
            if (CompOrNull<StationMemberComponent>(xform.GridUid)?.Station != chosenStation ||
                !_random.Prob(target.Chance)) // imp
                continue;
            // begin imp edit again
            var ev = new IonStormEvent();
            RaiseLocalEvent(ent, ref ev);
            //     _ionStorm.IonStormTarget((ent, lawBound, target));
        }
    }
}
[ByRefEvent]
public record struct IonStormEvent(bool Adminlog = true);
