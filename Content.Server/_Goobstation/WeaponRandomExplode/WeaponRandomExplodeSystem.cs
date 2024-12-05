using Robust.Shared.Random;
using Content.Shared.Weapons.Ranged.Events;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.Components;

namespace Content.Server._Goobstation.WeaponRandomExplode
{
    public sealed class WeaponRandomExplodeSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly ExplosionSystem _explosionSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<WeaponRandomExplodeComponent, ShotAttemptedEvent>(OnShot);
        }

        private void OnShot(EntityUid uid, WeaponRandomExplodeComponent component, ShotAttemptedEvent args)
        {
            if (component.explosionChance <= 0)
                return;

            TryComp<BatteryComponent>(uid, out var battery);
            if (battery == null || battery.CurrentCharge <= 0)
                return;

            if (_random.Prob(component.explosionChance))
            {
                var intensity = 1;
                var reduction = 1; //#funkystation type shit
                if (component.reduction != null)
                {
                    reduction = Convert.ToInt32(component.reduction);
                }
                if (component.multiplyByCharge > 0)
                {
                    intensity = Convert.ToInt32(component.multiplyByCharge * (battery.CurrentCharge / (100 * reduction)));
                }

                _explosionSystem.QueueExplosion(
                    (EntityUid) uid,
                    typeId: "Default",
                    totalIntensity: intensity,
                    slope: (5 / reduction),
                    maxTileIntensity: (10 / reduction));
                if (component.destroyGun) //#funkystation part of comp or something idk go away tay i dont wanna do this shit
                {
                    QueueDel(uid);
                }
            }
        }
    }
}
