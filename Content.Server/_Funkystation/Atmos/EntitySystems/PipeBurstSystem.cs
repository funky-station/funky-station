// SPDX-FileCopyrightText: 2025 QueerCats <jansencheng3@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Funkystation.Atmos.Components;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Destructible;
using Content.Server.Explosion.EntitySystems;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.Nodes;
using Content.Shared.Atmos;
using Content.Shared.CCVar;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using JetBrains.Annotations;
using Robust.Shared.Collections;
using Robust.Shared.Serialization.Manager.Exceptions;

namespace Content.Server._Funkystation.Atmos.EntitySystems
{
    [UsedImplicitly]
    public class PipeBurstSystem : EntitySystem
    {
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audioSys = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly SharedDestructibleSystem _destructibleSystem = default!;
        [Dependency] private readonly ExplosionSystem _explosions = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        private float _maxExplosionRange;

        public override void Initialize()
        {
            Subs.CVar(_cfg, CCVars.AtmosTankFragment, UpdateMaxRange, true);
            base.Initialize();
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<PipeBurstComponent>();
            var queryList = new ValueList<(EntityUid, PipeBurstComponent)>();
            while (query.MoveNext(out var uid, out var comp))
            {
                queryList.Add((uid, comp));
            }

            foreach (var entity in queryList)
            {
                CheckStatus(entity);
            }
        }
        private void UpdateMaxRange(float value)
        {
            _maxExplosionRange = value;
        }

        public void CheckStatus(Entity<PipeBurstComponent> ent)
        {
            var (uid, component) = ent;
            GasMixture? air = null;
            PipeNode? pipe = null;
            if (!TryComp<NodeContainerComponent>(uid, out var nodeContainer))
                return;
            foreach (var node in nodeContainer.Nodes.Values)
            {
                if (node is not PipeNode pipeNode)
                    continue;

                air = pipeNode.Air;
                pipe = pipeNode;
            }

            if (air == null || pipe == null)
                return;

            var pressure = air.Pressure;

            if (pressure > component.PipeLeakPressure)
            {
                var environment = _atmosphereSystem.GetContainingMixture(uid, false, true);
                if (environment != null)
                {
                    var leakedGas = air.RemoveRatio(0.25f);
                    _atmosphereSystem.Merge(environment, leakedGas);
                }
            }

            if (pressure > component.PipeRupturePressure)
            {
                var environment = _atmosphereSystem.GetContainingMixture(uid, false, true);
                if (environment != null)
                    _atmosphereSystem.Merge(environment, air);

                _audioSys.PlayPvs(component.RuptureSound, Transform(uid).Coordinates, AudioParams.Default.WithVariation(0.125f));
                TryComp<DamageableComponent>(uid, out var damageable);
                _damageableSystem.TryChangeDamage(uid, component.RuptureDamage);
            }

            if (pressure > component.PipeFragmentPressure && _maxExplosionRange > 0)
            {
                // Give the gas a chance to build up more pressure.
                for (var i = 0; i < 3; i++)
                {
                    _atmosphereSystem.React(air, pipe);
                }

                pressure = air.Pressure;
                var range = MathF.Sqrt((pressure - component.PipeFragmentPressure) / component.PipeFragmentScale);

                // Let's cap the explosion, yeah?
                // !1984
                range = Math.Min(Math.Min(range, PipeBurstComponent.MaxExplosionRange), _maxExplosionRange);

                _explosions.TriggerExplosive(uid, radius: range);
            }
        }
    }
}
