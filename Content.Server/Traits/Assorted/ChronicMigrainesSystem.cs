// SPDX-FileCopyrightText: 2025 Mora <46364955+TrixxedHeart@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Popups;
using Content.Shared.Traits.Assorted;
using Content.Shared.Mindshield.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;
namespace Content.Server.Traits.Assorted;

/// <summary>
/// This handles chronic migraines, causing the affected to experience random debilitating migraine episodes.
/// </summary>
public sealed class ChronicMigrainesSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private EntityQuery<NeuroAversionComponent> _neuroAversionQuery;
    private EntityQuery<MindShieldComponent> _mindShieldQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChronicMigrainesComponent, ComponentStartup>(SetupChronicMigraines);
        SubscribeLocalEvent<ActorComponent, ComponentStartup>(OnActorStartup);

        _neuroAversionQuery = GetEntityQuery<NeuroAversionComponent>();
        _mindShieldQuery = GetEntityQuery<MindShieldComponent>();
    }

    private void OnActorStartup(EntityUid uid, ActorComponent component, ComponentStartup args)
    {
        // If this entity currently has a migraine effect, show the popup
        if (TryComp<MigraineComponent>(uid, out _))
        {
            var msg = Loc.GetString("trait-chronic-migraines-start");
            _popup.PopupEntity(msg, uid, uid, PopupType.MediumCaution);
        }
    }

    private void SetupChronicMigraines(EntityUid uid, ChronicMigrainesComponent component, ComponentStartup args)
    {
        component.NextMigraineTime = _random.NextFloat(component.TimeBetweenMigraines.X, component.TimeBetweenMigraines.Y);
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ChronicMigrainesComponent>();
        while (query.MoveNext(out var uid, out var migraines))
        {
            // Skip chronic migraines processing if entity has the NeuroAversion trait and is Mindshielded
            // In this case, NeuroAversionSystem handles all migraine scheduling with trait interaction
            if (_neuroAversionQuery.HasComponent(uid) && _mindShieldQuery.HasComponent(uid))
                continue;

            migraines.NextMigraineTime -= frameTime;

            if (migraines.NextMigraineTime >= 0)
                continue;

            // Don't start a new migraine if one is already active
            if (HasComp<MigraineComponent>(uid))
                continue;

            // Set the new time for next incident
            migraines.NextMigraineTime += _random.NextFloat(migraines.TimeBetweenMigraines.X, migraines.TimeBetweenMigraines.Y);

            var duration = _random.NextFloat(migraines.MigraineDuration.X, migraines.MigraineDuration.Y);

            // Make sure the episode time doesn't cut into the time to next incident
            migraines.NextMigraineTime += duration;

            var msg = Loc.GetString("trait-chronic-migraines-start");
            _popup.PopupEntity(msg, uid, uid, PopupType.MediumCaution);

            // Show to other nearby players
            var othersMsg = Loc.GetString("trait-chronic-migraines-others", ("target", uid));
            _popup.PopupEntity(othersMsg, uid, Filter.PvsExcept(uid), true, PopupType.Medium);

            var migraineComp = AddComp<MigraineComponent>(uid);
            migraineComp.Duration = duration;
            migraineComp.FadeOutDuration = migraines.FadeOutDuration;
        }
    }
}
