// SPDX-FileCopyrightText: 2025 Patrycja <git@ptrcnull.me>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Administration.Logs;
using Content.Shared.Body.Organ;
using Content.Shared.Database;
using Content.Shared.EntityEffects;

using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Body.Systems
{
    /// <summary>
    /// Applies configured EntityEffects stored on ConstantHealingComponent periodically to the body entity
    /// attached to the organ (same responsibilities as before but using the new EntityEffect API).
    /// </summary>
    public sealed class ConstantHealingSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
        private EntityQuery<OrganComponent> _organQuery = default!;

        public override void Initialize()
        {
            base.Initialize();
            _organQuery = GetEntityQuery<OrganComponent>();

            SubscribeLocalEvent<ConstantHealingComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<ConstantHealingComponent, EntityUnpausedEvent>(OnUnpaused);
        }

        private void OnMapInit(Entity<ConstantHealingComponent> ent, ref MapInitEvent args)
        {
            ent.Comp.NextUpdate = _gameTiming.CurTime + ent.Comp.UpdateInterval;
        }

        private void OnUnpaused(Entity<ConstantHealingComponent> ent, ref EntityUnpausedEvent args)
        {
            ent.Comp.NextUpdate += args.PausedTime;
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<ConstantHealingComponent>();

            while (query.MoveNext(out var uid, out var comp))
            {
                if (_gameTiming.CurTime < comp.NextUpdate)
                    continue;

                comp.NextUpdate += comp.UpdateInterval;

                // Resolve the organ component on this entity (if any)
                Entity<ConstantHealingComponent, OrganComponent?> ent = (uid, comp);
                _organQuery.Resolve(ent, ref ent.Comp2, logMissing: false);

                if (ent.Comp2?.Body is not EntityUid body)
                    continue;

                // For each configured EntityEffect on the component, evaluate probability and raise it.
                foreach (var effect in comp.Effects)
                {
                    if (effect == null)
                        continue;

                    // Probability check:
                    // effect.Probability defaults to 1.0f per the class; treat <= 0 as never, >=1 as always.
                    if (effect.Probability <= 0f)
                        continue;

                    if (effect.Probability < 1f)
                    {
                        if (_random.NextDouble() > effect.Probability)
                            continue;
                    }

                    // Optional admin logging for effects that have an Impact set.
                    if (effect.Impact is not null)
                    {
                        _adminLogger.Add(
                            effect.LogType,
                            effect.Impact!.Value,
                            $"Constant healing effect {effect.GetType().Name:effect}"
                            + $" of organ {ent.Comp2.SlotId:organ}"
                            + $" applied on entity {body:entity}"
                            + $" at {Transform(body).Coordinates:coordinates}"
                        );
                    }
                }
            }
        }
    }
}
