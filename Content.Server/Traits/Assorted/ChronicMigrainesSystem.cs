// SPDX-License-Identifier: MIT

using Content.Shared.Popups;
using Content.Shared.Traits.Assorted;
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

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChronicMigrainesComponent, ComponentStartup>(SetupChronicMigraines);
        SubscribeLocalEvent<ActorComponent, ComponentStartup>(OnActorStartup);
        SubscribeLocalEvent<MigraineComponent, ComponentShutdown>(OnMigraineShutdown);
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

    private void OnMigraineShutdown(EntityUid uid, MigraineComponent component, ComponentShutdown args)
    {
        if (!HasComp<ChronicMigrainesComponent>(uid))
            return;
        
        if (component.IsFading && !TerminatingOrDeleted(uid))
        {
            var msg = Loc.GetString("trait-chronic-migraines-end");
            _popup.PopupEntity(msg, uid, uid, PopupType.Medium);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ChronicMigrainesComponent>();
        while (query.MoveNext(out var uid, out var migraines))
        {
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

            var migraineComp = AddComp<MigraineComponent>(uid);
            migraineComp.Duration = duration;
            migraineComp.FadeOutDuration = migraines.FadeOutDuration;
        }
    }
}
