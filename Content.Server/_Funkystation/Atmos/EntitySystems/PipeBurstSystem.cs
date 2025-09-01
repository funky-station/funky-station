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
        public static int MaxTick = 30; // Once per second

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
            if (!TryComp<NodeContainerComponent>(uid, out var nodeContainer))
                return;
            foreach (var node in nodeContainer.Nodes.Values)
            {
                if (node is not PipeNode pipeNode)
                    continue;

                var air = pipeNode.Air;
                var pressure = air.Pressure;
                var pipe = pipeNode;

                var environment = _atmosphereSystem.GetContainingMixture(uid, false, true);

                if (environment == null)
                    continue;
                var airPressure = environment.Pressure;

                // Get absolute difference
                var pressureDiff = Math.Abs(pressure - airPressure);

                // Leak behaviour
                if (pressureDiff < component.PipeLeakPressure)
                    continue;
                switch (pressure > airPressure) // If pipe is greater pressure that atmosphere, leak gas, otherwise reverse
                {
                    case true:
                        _atmosphereSystem.Merge(environment, air.RemoveRatio(0.25f));
                        break;
                    case false:
                        _atmosphereSystem.Merge(air, environment.RemoveRatio(0.25f));
                        break;
                }

                // Rupture behaviour
                if (pressureDiff < component.PipeRupturePressure)
                {
                    component.Ticker = 0;
                    continue;
                }

                if (component.Ticker < MaxTick) // Limit rate check
                {
                    component.Ticker++;
                    continue;
                }
                component.Ticker = 0;
                _audioSys.PlayPvs(component.RuptureSound, Transform(uid).Coordinates, AudioParams.Default.WithVariation(0.125f));
                _damageableSystem.TryChangeDamage(uid, component.RuptureDamage);

                // Explosion behaviour
                if (pressureDiff < component.PipeFragmentPressure || _maxExplosionRange < 0)
                    continue;

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
