using Content.Shared.Interaction;
using Content.Server.Machines.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Genetics.Components;
using Robust.Shared.Timing;
using Robust.Shared.Random;
using Content.Shared.Access.Components;
using Content.Server.EntityEffects.Effects;

namespace Content.Server.Machines.EntitySystems
{

    public sealed class GeneManipulatorSystem : EntitySystem
    {
        [Dependency] private readonly PowerReceiverSystem _power = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<GeneManipulatorComponent, InteractUsingEvent>(OnInteractUsing);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var time = _timing.CurTime;

            var query = EntityQueryEnumerator<GeneManipulatorComponent>();

            while (query.MoveNext(out var uid, out var gene))
            {
                if (gene.CooldownNext <= time)
                {
                    gene.CooldownNext = time + gene.CooldownTime;

                    gene.Cooldown = false;

                }
            }
        }

        private void OnInteractUsing(EntityUid uid, GeneManipulatorComponent comp, InteractUsingEvent args)
        {
            if (args.Handled)
                return;
            if (!_power.IsPowered(uid))
                return;
            if (comp.Cooldown == true)
                return;

            if (TryComp<InjectionPresetComponent>(args.Used, out var injector))
                if (injector != null)
                {
                    SpawnInject(uid, comp, injector);
                }

            comp.Cooldown = true;

            args.Handled = true;
        }

        private void SpawnInject(EntityUid uid, GeneManipulatorComponent comp, InjectionPresetComponent injector)
        { //Get fucking ready for another hardcode check. I promise this isnt torture for the sake of pushing mrp taydumbo (it is)
            if (injector.AcidVomit)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.AcidVomit = true;
            }
            if (injector.BloodVomit)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.BloodVomit = true;
            }
            if (injector.BlueLight)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.BlueLight = true;
            }
            if (injector.BreathingImmune)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.BreathingImmune = true;
            }
            if (injector.BZFarter)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.BZFarter = true;
            }
            if (injector.Clumsy)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.Clumsy = true;
            }
            if (injector.FireSkin)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.FireSkin = true;
            }
            if (injector.Light)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.Light = true;
            }
            if (injector.OkayAccent)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.OkayAccent = true;
            }
            if (injector.PlasmaFarter)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.PlasmaFarter = true;
            }
            if (injector.PressureImmune)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.PressureImmune = true;
            }
            if (injector.Prickmode)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.Prickmode = true;
            }
            if (injector.RadiationImmune)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.RadiationImmune = true;
            }
            if (injector.RedLight)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.RedLight = true;
            }
            if (injector.RGBLight)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.RGBLight = true;
            }
            if (injector.TempImmune)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.TempImmune = true;
            }
            if (injector.TritFarter)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.TritFarter = true;
            }
            if (injector.Twitch)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.Twitch = true;
            }
            if (injector.Vomit)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.Vomit = true;
            }

            EntityManager.DeleteEntity(injector.Owner); //deprecate my left pinky toe, idgaf
        }
    }
}
