using Content.Server._Funkystation.Atmos.Components;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.CCVar;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;

namespace Content.Server._Funkystation.Atmos.EntitySystems
{
    [UsedImplicitly]
    public class PipeBurstSystem : EntitySystem
    {
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly ExplosionSystem _explosions = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        private float _maxExplosionRange;

        public override void Initialize()
        {
            Subs.CVar(_cfg, CCVars.AtmosTankFragment, UpdateMaxRange, true);
            base.Initialize();
        }
        private void UpdateMaxRange(float value)
        {
            _maxExplosionRange = value;
        }

        public void CheckStatus(Entity<PipeBurstComponent> ent)
        {
            var (uid, component) = ent;
            if (component.Air == null)
                return;

            var pressure = component.Air.Pressure;

            if (pressure > component.PipeFragmentPressure && _maxExplosionRange > 0)
            {
                // Give the gas a chance to build up more pressure.
                for (var i = 0; i < 3; i++)
                {
                    _atmosphereSystem.React(component.Air, component);
                }

                pressure = component.Air.Pressure;
                var range = MathF.Sqrt((pressure - component.PipeFragmentPressure) / component.PipeFragmentScale);

                // Let's cap the explosion, yeah?
                // !1984
                range = Math.Min(Math.Min(range, GasPipeComponent.MaxExplosionRange), _maxExplosionRange);

                _explosions.TriggerExplosive(uid, radius: range);

                return;
            }

            if (pressure > component.PipeRupturePressure)
            {
                if (component.Integrity <= 0)
                {
                    var environment = _atmosphereSystem.GetContainingMixture(owner, false, true);
                    if (environment != null)
                        _atmosphereSystem.Merge(environment, component.Air);

                    _audioSys.PlayPvs(component.RuptureSound, Transform(uid).Coordinates, AudioParams.Default.WithVariation(0.125f));

                    QueueDel(uid);
                    return;
                }

                component.Integrity--;
                return;
            }

            if (pressure > component.PipeLeakPressure)
            {
                if (component.Integrity <= 0)
                {
                    var environment = _atmosphereSystem.GetContainingMixture(uid, false, true);
                    if (environment == null)
                        return;

                    var leakedGas = component.Air.RemoveRatio(0.25f);
                    _atmosphereSystem.Merge(environment, leakedGas);
                }
                else
                {
                    component.Integrity--;
                }

                return;
            }

            if (component.Integrity < 3)
                component.Integrity++;
        }
    }
}
