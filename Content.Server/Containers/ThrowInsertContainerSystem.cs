// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2025 GreyMario <mariomister541@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Containers;

public sealed class ThrowInsertContainerSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrowInsertContainerComponent, ThrowHitByEvent>(OnThrowCollide);
    }

    private void OnThrowCollide(Entity<ThrowInsertContainerComponent> ent, ref ThrowHitByEvent args)
    {
        var container = _containerSystem.GetContainer(ent, ent.Comp.ContainerId);

        if (!_containerSystem.CanInsert(args.Thrown, container))
            return;

        var beforeThrowArgs = new BeforeThrowInsertEvent(args.Thrown);
        RaiseLocalEvent(ent, ref beforeThrowArgs);

        if (beforeThrowArgs.Cancelled)
            return;

        // funkystation: roll twice if it's an "accurate" throw (the landing time is near current time) to increase hit chance
        var hitThrow = _random.Prob(ent.Comp.Probability);
        if (HasComp<ThrownItemComponent>(args.Thrown) && Comp<ThrownItemComponent>(args.Thrown).LandTime - _gameTiming.CurTime <= TimeSpan.FromSeconds(0.2))
            hitThrow |= _random.Prob(ent.Comp.Probability);

        if (!hitThrow)
        {
            _audio.PlayPvs(ent.Comp.MissSound, ent);
            _popup.PopupEntity(Loc.GetString(ent.Comp.MissLocString), ent);
            return;
        }

        if (!_containerSystem.Insert(args.Thrown, container))
            throw new InvalidOperationException("Container insertion failed but CanInsert returned true");

        _audio.PlayPvs(ent.Comp.InsertSound, ent);

        if (args.Component.Thrower != null)
            _adminLogger.Add(LogType.Landed, LogImpact.Low, $"{ToPrettyString(args.Thrown)} thrown by {ToPrettyString(args.Component.Thrower.Value):player} landed in {ToPrettyString(ent)}");
    }
}

/// <summary>
/// Sent before the insertion is made.
/// Allows preventing the insertion if any system on the entity should need to.
/// </summary>
[ByRefEvent]
public record struct BeforeThrowInsertEvent(EntityUid ThrownEntity, bool Cancelled = false);
