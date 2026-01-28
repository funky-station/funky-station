// SPDX-FileCopyrightText: 2026 Mora <46364955+TrixxedHeart@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Popups;
using Content.Shared.Traits.Assorted;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Systems;
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
    [Dependency] private readonly MobStateSystem _mobState = default!;

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
        var seconds = _random.NextFloat((float)component.TimeBetweenMigraines.Min.TotalSeconds, (float)component.TimeBetweenMigraines.Max.TotalSeconds);
        component.NextMigraineTime = TimeSpan.FromSeconds(seconds);
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ChronicMigrainesComponent>();
        while (query.MoveNext(out var uid, out var migraines))
        {
            // dead people cant have migraines
            if (_mobState.IsDead(uid))
                continue;

            // Skip chronic migraines processing if entity has the NeuroAversion trait and is Mindshielded
            // In this case, NeuroAversionSystem handles all migraine scheduling with trait interaction
            if (_neuroAversionQuery.HasComponent(uid) && _mindShieldQuery.HasComponent(uid))
                continue;

            migraines.NextMigraineTime -= TimeSpan.FromSeconds(frameTime);
            Dirty(uid, migraines);

            if (migraines.NextMigraineTime > TimeSpan.Zero)
                continue;

            // Don't start a new migraine if one is already active
            if (HasComp<MigraineComponent>(uid))
                continue;

            // Pick new migraine time
            var nextMigraineSeconds = _random.NextFloat((float)migraines.TimeBetweenMigraines.Min.TotalSeconds, (float)migraines.TimeBetweenMigraines.Max.TotalSeconds);
            migraines.NextMigraineTime = TimeSpan.FromSeconds(nextMigraineSeconds);
            var durationSeconds = _random.NextFloat((float)migraines.MigraineDuration.Min.TotalSeconds, (float)migraines.MigraineDuration.Max.TotalSeconds);
            var duration = TimeSpan.FromSeconds(durationSeconds);
            Dirty(uid, migraines);

            var msg = Loc.GetString("trait-chronic-migraines-start");
            _popup.PopupEntity(msg, uid, uid, PopupType.MediumCaution);

            // Show to other nearby players
            var othersMsg = Loc.GetString("trait-chronic-migraines-others", ("target", uid));
            _popup.PopupEntity(othersMsg, uid, Filter.PvsExcept(uid), true, PopupType.Medium);

            var migraineComp = AddComp<MigraineComponent>(uid);
            migraineComp.Duration = (float)duration.TotalSeconds;
            migraineComp.FadeOutDuration = migraines.FadeOutDuration;

            // Make sure the episode time doesn't cut into the time to next incident
            migraines.NextMigraineTime += duration;
        }
    }
}
