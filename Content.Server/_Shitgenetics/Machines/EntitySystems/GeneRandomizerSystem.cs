using Content.Shared.Interaction;
using Content.Server.Machines.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Genetics.Components;
using Robust.Shared.Timing;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server.Machines.EntitySystems
{

    public sealed class GeneRandomizerSystem : EntitySystem
    {
        [Dependency] private readonly PowerReceiverSystem _power = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<GeneRandomizerComponent, InteractHandEvent>(OnInteractHand);


        }

        public override void Update(float frameTime) //tony shitmin save me from this feral shitcode, tony shitmin, tony shitmin please
        {
            base.Update(frameTime);

            var time = _timing.CurTime;

            var query = EntityQueryEnumerator<GeneRandomizerComponent>();

            while (query.MoveNext(out var uid, out var gene))
            {
                if (gene.CooldownNext <= time)
                {
                    gene.CooldownNext = time + gene.CooldownTime;

                    gene.Cooldown = false;

                }
            }
        }

        private void OnInteractHand(EntityUid uid, GeneRandomizerComponent comp, InteractHandEvent args)
        {
            if (args.Handled)
                return;
            if (!_power.IsPowered(uid))
                return;
            if (comp.Cooldown == true)
                return;

            string ent = "geneinjector"; //yummers

            var inject = Spawn(ent, Transform(uid).Coordinates);
            RandomGene(uid, comp, inject);

            comp.Cooldown = true;

            args.Handled = true;
        }

        private void RandomGene(EntityUid uid, GeneRandomizerComponent comp, EntityUid inject)
        {
            var injecter = EntityManager.GetComponent<InjectionPresetComponent>(inject);

            for (var i = 0; i < 3; i++)//basically this is how this shit gens random mutations, a hardcode messy fucking nightmare
            {
                var random = _random.Next(1, 41);//manually have to add each gene and assign a number. It works, BUT PLEASE, find a better way to do this i actually hate my LIFE.

                if (random == 1) injecter.AcidVomit = true;
                if (random == 2) injecter.Anchorable = true;
                if (random == 3) injecter.BackwardsAccent = true;
                if (random == 4) injecter.BigSize = true;
                if (random == 5) injecter.Blindness = true;
                if (random == 6) injecter.BloodVomit = true;
                if (random == 7) injecter.BlueLight = true;
                if (random == 8) injecter.Vomit = true;
                if (random == 9) injecter.BZFarter = true;
                if (random == 10) injecter.Cancer = true;
                if (random == 11) injecter.Clumsy = true;
                if (random == 12) injecter.EMPer = true;
                if (random == 13) injecter.Explode = true;
                if (random == 14) injecter.EyeDamage = true;
                if (random == 15) injecter.FireSkin = true;
                if (random == 16) injecter.High = true;
                if (random == 17) injecter.Item = true;
                if (random == 18) injecter.Leprosy = true;
                if (random == 19) injecter.Light = true;
                if (random == 20) injecter.LubeVomit = true;
                if (random == 21) injecter.MobsterAccent = true;
                if (random == 22) injecter.OhioAccent = true;
                if (random == 23) injecter.OkayAccent = true;
                if (random == 24) injecter.OWOAccent = true;
                if (random == 25) injecter.PlasmaFarter = true;
                if (random == 26) injecter.PressureImmune = true;
                if (random == 27) injecter.Prickmode = true;
                if (random == 28) injecter.RadiationImmune = true;
                if (random == 29) injecter.RedLight = true;
                if (random == 30) injecter.RGBLight = true;
                if (random == 31) injecter.ScrambleAccent = true;
                if (random == 32) injecter.SelfHeal = true;
                if (random == 33) injecter.Slippy = true;
                if (random == 34) injecter.SmallSize = true;
                if (random == 35) injecter.StutterAccent = true;
                if (random == 36) injecter.TempImmune = true;
                if (random == 37) injecter.TinySize = true;
                if (random == 38) injecter.TritFarter = true;
                if (random == 39) injecter.Twitch = true;
                if (random == 40) injecter.UberSlippy = true;
            }
        }
    }
}
