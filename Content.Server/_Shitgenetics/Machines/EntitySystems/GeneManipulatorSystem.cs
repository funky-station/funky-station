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

            if (TryComp<GeneinjectorComponent>(args.Used, out var geneinject))
                if (geneinject != null)
                {
                    if (TryComp<InjectionPresetComponent>(args.Used, out var injector))
                        if (injector != null)
                        {
                            SpawnInject(uid, comp, geneinject, args, injector);
                        }
                }

            if (TryComp<GeneSampleComponent>(args.Used, out var genesample))
                if (genesample != null)
                {
                    if (TryComp<InjectionPresetComponent>(args.Used, out var injector))
                        if (injector != null)
                        {
                            CreateInject(uid, comp, genesample, args, injector);
                        }
                }


            comp.Cooldown = true;

            args.Handled = true;
        }

        private void SpawnInject(EntityUid uid, GeneManipulatorComponent comp, GeneinjectorComponent geneinject, InteractUsingEvent args, InjectionPresetComponent injector)
        { //Get fucking ready for another hardcode check. I promise this isnt torture for the sake of pushing mrp taydumbo (it is)

            if (injector.AcidVomit)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.AcidVomit = true;
            }
            if (injector.Anchorable)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.Anchorable = true;
            }
            if (injector.BackwardsAccent)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.BackwardsAccent = true;
            }
            if (injector.BigSize)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.BigSize = true;
            }
            if (injector.Blindness)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.Blindness = true;
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
            if (injector.BZFarter)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.BZFarter = true;
            }
            if (injector.Cancer)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.Cancer = true;
            }
            if (injector.Clumsy)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.Clumsy = true;
            }
            if (injector.EMPer)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.EMPer = true;
            }
            if (injector.Explode)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.Explode = true;
            }
            if (injector.EyeDamage)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.EyeDamage = true;
            }
            if (injector.FireSkin)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.FireSkin = true;
            }
            if (injector.High)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.High = true;
            }
            if (injector.Item)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.Item = true;
            }
            if (injector.Leprosy)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.Leprosy = true;
            }
            if (injector.Light)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.Light = true;
            }
            if (injector.LubeVomit)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.LubeVomit = true;
            }
            if (injector.MobsterAccent)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.MobsterAccent = true;
            }
            if (injector.OhioAccent)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.OhioAccent = true;
            }
            if (injector.OkayAccent)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.OkayAccent = true;
            }
            if (injector.OWOAccent)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.OWOAccent = true;
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
            if (injector.ScrambleAccent)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.ScrambleAccent = true;
            }
            if (injector.SelfHeal)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.SelfHeal = true;
            }
            if (injector.Slippy)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.Slippy = true;
            }
            if (injector.SmallSize)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.SmallSize = true;
            }
            if (injector.StutterAccent)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.StutterAccent = true;
            }
            if (injector.TempImmune)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.TempImmune = true;
            }
            if (injector.TinySize)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.TinySize = true;
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
            if (injector.UberSlippy)
            {
                string ent = "geneinjector";
                var inject = Spawn(ent, Transform(uid).Coordinates);
                var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);
                injectclone.UberSlippy = true;
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

        private void CreateInject(EntityUid uid, GeneManipulatorComponent comp, GeneSampleComponent genesample, InteractUsingEvent args, InjectionPresetComponent injector)
        { //time to extract le blood sample
            string ent = "geneinjector";
            var inject = Spawn(ent, Transform(uid).Coordinates);
            var injectclone = EntityManager.GetComponent<InjectionPresetComponent>(inject);

            if (injector.AcidVomit) injectclone.AcidVomit = true; //its 3 am, im fucking tired, im just gonna hardcode it, im so sorry taydeo, I have dishonored the john space bloodline.
            if (injector.Anchorable) injectclone.Anchorable = true;
            if (injector.BackwardsAccent) injectclone.BackwardsAccent = true;
            if (injector.BigSize) injectclone.BigSize = true;
            if (injector.Blindness) injectclone.Blindness = true;
            if (injector.BloodVomit) injectclone.BloodVomit = true;
            if (injector.BlueLight) injectclone.BlueLight = true;
            if (injector.BZFarter) injectclone.BZFarter = true;
            if (injector.Cancer) injectclone.Cancer = true;
            if (injector.Clumsy) injectclone.Clumsy = true;
            if (injector.EMPer) injectclone.EMPer = true;
            if (injector.Explode) injectclone.Explode = true;
            if (injector.EyeDamage) injectclone.EyeDamage = true;
            if (injector.FireSkin) injectclone.FireSkin = true;
            if (injector.High) injectclone.High = true;
            if (injector.Item) injectclone.Item = true;
            if (injector.Leprosy) injectclone.Leprosy = true;
            if (injector.Light) injectclone.Light = true;
            if (injector.LubeVomit) injectclone.LubeVomit = true;
            if (injector.MobsterAccent) injectclone.MobsterAccent = true;
            if (injector.OhioAccent) injectclone.OhioAccent = true;
            if (injector.OkayAccent) injectclone.OkayAccent = true;
            if (injector.OWOAccent) injectclone.OWOAccent = true;
            if (injector.PlasmaFarter) injectclone.PlasmaFarter = true;
            if (injector.PressureImmune) injectclone.PressureImmune = true;
            if (injector.Prickmode) injectclone.Prickmode = true;
            if (injector.RadiationImmune) injectclone.RadiationImmune = true;
            if (injector.RedLight) injectclone.RedLight = true;
            if (injector.RGBLight) injectclone.RGBLight = true;
            if (injector.ScrambleAccent) injectclone.ScrambleAccent = true;
            if (injector.SelfHeal) injectclone.SelfHeal = true;
            if (injector.Slippy) injectclone.Slippy = true;
            if (injector.SmallSize) injectclone.SmallSize = true;
            if (injector.StutterAccent) injectclone.StutterAccent = true;
            if (injector.TempImmune) injectclone.TempImmune = true;
            if (injector.TinySize) injectclone.TinySize = true;
            if (injector.TritFarter) injectclone.TritFarter = true;
            if (injector.Twitch) injectclone.Twitch = true;
            if (injector.UberSlippy) injectclone.UberSlippy = true;
            if (injector.Vomit) injectclone.Vomit = true;

            EntityManager.DeleteEntity(injector.Owner); //deprecate this, deprecate that, why dont you deprecate some bitches
        }
    }
}
