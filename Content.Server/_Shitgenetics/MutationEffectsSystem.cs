using Content.Shared.Mutations;
using Content.Shared.Jittering;
using Content.Shared.Light.Components;
using Content.Server.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.StatusEffect;
using Content.Shared.Fluids;
using Content.Server.Stunnable;
using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Chemistry.Components;
using Robust.Shared.Random;
using Content.Shared.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Clumsy;
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Temperature.Components;
using Content.Shared.Temperature.Components;
using Content.Server.Radiation.Components;
using Content.Server.Speech.Components;
using Content.Server.Chat.Systems;
using Content.Shared.Genetics.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Chemistry.Reagent;


public sealed partial class MututationSystem : EntitySystem
{
    //star wars intro or something i just stole john stations joke lmfao pwned fatso
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedPointLightSystem _pointlight = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Update(float frameTime) //erm... we gotta update the stupid bool checks for all the effects to keep em updated. aint called shitgenetics for no reason.
    {
        base.Update(frameTime);

        foreach (var comp in EntityManager.EntityQuery<MutationComponent>())
        {
            var uid = comp.Owner; // go fuck yourself what is this SHIT

            CycleEffects(uid, comp);


            if (comp.Twitch) //thugging this shit out yandev style
            {
                Twitching(uid, comp);
            }
            if (comp.Light)
            {
                Light(uid, comp);
            }
            if (comp.RedLight)
            {
                RedLight(uid, comp);
            }
            if (comp.BlueLight)
            {
                BlueLight(uid, comp);
            }
            if (comp.RGBLight)
            {
                RGBLight(uid, comp);
            }
            if (comp.Clumsy)
            {
                Clumsy(uid, comp);
            }
            if (comp.TempImmune)
            {
                TempImmune(uid, comp);
            }
            if (comp.OkayAccent)
            {
                OkayAccent(uid, comp);
            }
            if (comp.TempImmune)
            {
                TempImmune(uid, comp);
            }
            if (comp.PressureImmune)
            {
                PressureImmune(uid, comp);
            }
            if (comp.RadiationImmune)
            {
                RadImmune(uid, comp);
            }
            if (comp.Prickmode)
            {
                PrickAccent(uid, comp);
            }


            if (comp.Cancel)
            {
                EntityManager.RemoveComponent<MutationComponent>(uid); // delete self after tick
            }
        }
    }

    public void CycleEffects(EntityUid uid, MutationComponent comp) // this fuckin bullshit is for effects that i skibidi my sigma not per every tick cuz thats fucked
    {

        comp.MutationUpdateTimer += 1;
        if (comp.MutationUpdateTimer >= comp.MutationUpdateCooldown)
        {
            comp.MutationUpdateTimer = 0;

            if (comp.Amount > 6) //todo: change the 6 to a cvar. If i forget, its probably on purpose cuz i hate THIS FUCKING SERVER MAN
            {
                GeneticPain(uid, comp);
            }

            if (comp.Vomit) //fent warriors please rise up
            {
                Vomit(uid, comp);
            }
            if (comp.BloodVomit)
            {
                VomitBlood(uid, comp);
            }
            if (comp.AcidVomit)
            {
                VomitAcid(uid, comp);
            }
            if (comp.PlasmaFarter)
            {
                PlasmaFart(uid, comp);
            }
            if (comp.TritFarter)
            {
                TritFart(uid, comp);
            }
            if (comp.BZFarter)
            {
                BZFart(uid, comp);
            }
            if (comp.FireSkin)
            {
                FireSkin(uid, comp);
            }
            if (comp.Prickmode)
            {
                PrickSay(uid, comp);
            }
            if (comp.SelfHeal)
            {
                SelfHeal(uid, comp);
            }
        }
    }

    #region Visual
    private void Twitching(EntityUid uid, MutationComponent comp)
    {
        _jitter.DoJitter(uid, TimeSpan.FromSeconds(.5f), true, amplitude: 5, frequency: 10);
    }

    private void Light(EntityUid uid, MutationComponent comp)
    {
        _pointlight.EnsureLight(uid);
        if (comp.Cancel) _pointlight.RemoveLightDeferred(uid);
    }

    private void RedLight(EntityUid uid, MutationComponent comp)
    {
        _pointlight.EnsureLight(uid);
        _pointlight.SetColor(uid, new Color(255, 0, 0));
        if (comp.Cancel) _pointlight.RemoveLightDeferred(uid);
    }

    private void BlueLight(EntityUid uid, MutationComponent comp)
    {
        _pointlight.EnsureLight(uid);
        _pointlight.SetColor(uid, new Color(0, 0, 255));
        if (comp.Cancel) _pointlight.RemoveLightDeferred(uid);
    }

    private void RGBLight(EntityUid uid, MutationComponent comp)
    {
        _pointlight.EnsureLight(uid);
        EnsureComp<RgbLightControllerComponent>(uid);
        if (comp.Cancel)
        {
            _pointlight.RemoveLightDeferred(uid);
            RemComp<RgbLightControllerComponent>(uid);
        }
    }
    #endregion

    #region Emitting Stuff

    private void Vomit(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 3);

        if (random == 2)
        {
            if (TryComp<StatusEffectsComponent>(uid, out var status))
                _stun.TrySlowdown(uid, TimeSpan.FromSeconds(1.5f), true, 0.5f, 0.5f, status);

            var solution = new Solution();

            var vomitAmount = 10f;
            solution.AddReagent("Vomit", vomitAmount);

            _puddle.TrySplashSpillAt(uid, Transform(uid).Coordinates, solution, out _);

            _popup.PopupEntity(Loc.GetString("disease-vomit", ("person", Identity.Entity(uid, EntityManager))), uid);
        }
    }

    private void VomitBlood(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 5);

        if (random == 4)
        {
            if (TryComp<StatusEffectsComponent>(uid, out var status))
                _stun.TrySlowdown(uid, TimeSpan.FromSeconds(1.5f), true, 0.5f, 0.5f, status);

            var solution = new Solution();

            var vomitAmount = 10f;
            solution.AddReagent("Blood", vomitAmount);

            _puddle.TrySplashSpillAt(uid, Transform(uid).Coordinates, solution, out _);

            _popup.PopupEntity(Loc.GetString("disease-vomit", ("person", Identity.Entity(uid, EntityManager))), uid);
        }
    }

    private void VomitAcid(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 6);

        if (random == 5)
        {
            if (TryComp<StatusEffectsComponent>(uid, out var status))
                _stun.TrySlowdown(uid, TimeSpan.FromSeconds(1.5f), true, 0.5f, 0.5f, status);

            var solution = new Solution();

            var vomitAmount = 25f;
            solution.AddReagent("PolytrinicAcid", vomitAmount);

            _puddle.TrySplashSpillAt(uid, Transform(uid).Coordinates, solution, out _);

            _popup.PopupEntity(Loc.GetString("disease-vomit", ("person", Identity.Entity(uid, EntityManager))), uid);
        }
    }

    private void PlasmaFart(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 6);

        if (random == 5)
        {
            var tilepos = _xform.GetGridOrMapTilePosition(uid, Transform(uid));
            var enumerator = _atmos.GetAdjacentTileMixtures(Transform(uid).GridUid!.Value, tilepos, false, false);
            while (enumerator.MoveNext(out var mix))
            {
                mix.AdjustMoles(Gas.Plasma, 10f);
                _popup.PopupEntity(Loc.GetString("gas-fart", ("person", Identity.Entity(uid, EntityManager))), uid);
            }
        }
    }

    private void TritFart(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 6);

        if (random == 5)
        {
            var tilepos = _xform.GetGridOrMapTilePosition(uid, Transform(uid));
            var enumerator = _atmos.GetAdjacentTileMixtures(Transform(uid).GridUid!.Value, tilepos, false, false);
            while (enumerator.MoveNext(out var mix))
            {
                mix.AdjustMoles(Gas.Tritium, 5f);
                _popup.PopupEntity(Loc.GetString("gas-fart", ("person", Identity.Entity(uid, EntityManager))), uid);
            }
        }
    }

    private void BZFart(EntityUid uid, MutationComponent comp) // could be peak if you get a bunch of monkeys in a single room with this
    {
        var random = (int) _rand.Next(1, 7);

        if (random == 6)
        {
            var tilepos = _xform.GetGridOrMapTilePosition(uid, Transform(uid));
            var enumerator = _atmos.GetAdjacentTileMixtures(Transform(uid).GridUid!.Value, tilepos, false, false);
            while (enumerator.MoveNext(out var mix))
            {
                mix.AdjustMoles(Gas.BZ, 25f);
                _popup.PopupEntity(Loc.GetString("gas-fart", ("person", Identity.Entity(uid, EntityManager))), uid);
            }
        }
    }

    #endregion

    #region Body Stuff
    private void Clumsy(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<ClumsyComponent>(uid);
        if (comp.Cancel) RemComp<ClumsyComponent>(uid);

    }

    private void FireSkin(EntityUid uid, MutationComponent comp) //i hope an ipc gets this and fucking DIES
    {
        var random = (int) _rand.Next(1, 3);

        if (random == 2)
        {
            _flammable.AdjustFireStacks(uid, 2.5f, ignite: true);
            _popup.PopupEntity(Loc.GetString("burst-flames", ("person", Identity.Entity(uid, EntityManager))), uid);

        }
    }
    private void TempImmune(EntityUid uid, MutationComponent comp)
    {
        if (TryComp<TemperatureComponent>(uid, out var temp))
        {
            temp.ColdDamageThreshold = -420f;
            temp.HeatDamageThreshold = 19841984f; //im sure this works :clueless:
            if (comp.Cancel)
            {
                temp.ColdDamageThreshold = 260f;
                temp.HeatDamageThreshold = 360f;
            }
        }
    }

    private void PressureImmune(EntityUid uid, MutationComponent comp)
    {
        if (TryComp<BarotraumaComponent>(uid, out var baro))
        {
            baro.HasImmunity = true;
            if (comp.Cancel)
            {
                baro.HasImmunity = false;
            }
        }
    }

    private void RadImmune(EntityUid uid, MutationComponent comp)
    {
        RemComp<RadiationReceiverComponent>(uid);
        if (comp.Cancel) EnsureComp<RadiationReceiverComponent>(uid);
    }

    private void SelfHeal(EntityUid uid, MutationComponent comp) // "This is better than I thought! JACKKKKKKKKPOOOOOTTTTTTT!!"
    {
        var solution = new Solution();
        solution.AddReagent("DoctorsDelight", 4f);
        if (_solution.TryGetInjectableSolution(uid, out var targetSolution, out var _))
            _solution.TryAddSolution(targetSolution.Value, solution);
    }

    #endregion

    #region Accents

    private void OkayAccent(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<OkayAccentComponent>(uid);
        if (comp.Cancel) RemComp<OkayAccentComponent>(uid);
    }

    private void PrickAccent(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<PrickAccentComponent>(uid);
        if (comp.Cancel) RemComp<PrickAccentComponent>(uid);
    }
    private void PrickSay(EntityUid uid, MutationComponent comp)
    {

        var random = (int) _rand.Next(1, 10);

        if (random == 8)
        {
            _chat.TrySendInGameICMessage(uid, Loc.GetString("mutation-coolit"), InGameICChatType.Speak, false);
        }
        if (random == 9)
        {
            _chat.TrySendInGameICMessage(uid, Loc.GetString("mutation-sayit"), InGameICChatType.Speak, false);
        }
    }

    #endregion

    private void GeneticPain(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 4);

        if (random == 3)
        {
            var prot = (ProtoId<DamageGroupPrototype>) "Genetic";
            var dmgtype = _proto.Index(prot);
            _damage.TryChangeDamage(uid, new DamageSpecifier(dmgtype, 5f), true);
            _popup.PopupEntity(Loc.GetString("genetic-pain", ("person", Identity.Entity(uid, EntityManager))), uid);
        }
    }
}
