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

namespace Content.Server.Body.Systems
{
    public sealed class ConstantHealingSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
        private EntityQuery<OrganComponent> _organQuery;

        public override void Initialize()
        {
            base.Initialize();

            _organQuery = GetEntityQuery<OrganComponent>();
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

                Entity<ConstantHealingComponent, OrganComponent?> ent = (uid, comp);
                _organQuery.Resolve(ent, ref ent.Comp2, logMissing: false);

                if (ent.Comp2?.Body is EntityUid body) {
                    var args = new EntityEffectBaseArgs(body, EntityManager);
                    foreach (var effect in comp.Effects)
                    {
                        if (!effect.ShouldApply(args, _random))
                            continue;

                        if (effect.ShouldLog)
                        {
                            _adminLogger.Add(
                                LogType.ReagentEffect,
                                effect.LogImpact,
                                $"Constant healing effect {effect.GetType().Name:effect}"
                                + $" of organ {ent.Comp2.SlotId:organ}"
                                + $" applied on entity {body:entity}"
                                + $" at {Transform(body).Coordinates:coordinates}"
                            );
                        }

                        effect.Effect(args);
                    }
                }
            }
        }
    }
}
