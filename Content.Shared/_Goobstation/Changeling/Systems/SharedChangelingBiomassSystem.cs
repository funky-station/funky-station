using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Alert;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Content.Shared.Stunnable;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Changeling.Systems;

public abstract class SharedChangelingBiomassSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly MobThresholdSystem _mob = default!;
    [Dependency] private readonly SharedBloodstreamSystem _blood = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    private EntityQuery<AbsorbedComponent> _absorbQuery;
    private EntityQuery<ChangelingIdentityComponent> _lingQuery;

    public override void Initialize()
    {
        base.Initialize();

        _absorbQuery = GetEntityQuery<AbsorbedComponent>();
        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();

        SubscribeLocalEvent<ChangelingBiomassComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ChangelingBiomassComponent, ComponentRemove>(OnRemoved);

        SubscribeLocalEvent<ChangelingBiomassComponent, RejuvenateEvent>(OnRejuvenate);
    }

    private void OnMapInit(Entity<ChangelingBiomassComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        ent.Comp.FirstWarnThreshold = ent.Comp.MaxBiomass * 0.75f;
        ent.Comp.SecondWarnThreshold = ent.Comp.MaxBiomass * 0.5f;
        ent.Comp.ThirdWarnThreshold = ent.Comp.MaxBiomass * 0.25f;

        if (_lingQuery.TryComp(ent, out var ling))
            ling.ChemicalRegenMultiplier += ent.Comp.ChemicalBoost;

        Cycle(ent);
    }

    private void OnRemoved(Entity<ChangelingBiomassComponent> ent, ref ComponentRemove args)
    {
        _alerts.ClearAlert(ent.Owner, ent.Comp.AlertId);

        if (_lingQuery.TryComp(ent, out var ling))
            ling.ChemicalRegenMultiplier -= ent.Comp.ChemicalBoost;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<ChangelingBiomassComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + comp.UpdateDelay;

            Cycle((uid, comp));
        }
    }

    private void Cycle(Entity<ChangelingBiomassComponent> ent)
    {
        UpdateBiomass(ent);

        // first
        if (!ent.Comp.FirstWarnReached
            && ent.Comp.Biomass <= ent.Comp.FirstWarnThreshold)
        {
            ent.Comp.FirstWarnReached = true;

            DoPopup(ent, ent.Comp.FirstWarnPopup, PopupType.SmallCaution);
        }
        else if (ent.Comp.Biomass > ent.Comp.FirstWarnThreshold)
            ent.Comp.FirstWarnReached = false;

        // second
        if (!ent.Comp.SecondWarnReached
            && ent.Comp.Biomass <= ent.Comp.SecondWarnThreshold)
        {
            ent.Comp.SecondWarnReached = true;

            DoPopup(ent, ent.Comp.SecondWarnPopup, PopupType.MediumCaution);

            _stun.TryAddStunDuration(ent, ent.Comp.SecondWarnStun);
        }
        else if (ent.Comp.Biomass > ent.Comp.SecondWarnThreshold)
            ent.Comp.SecondWarnReached = false;

        // third
        if (!ent.Comp.ThirdWarnReached
            && ent.Comp.Biomass <= ent.Comp.ThirdWarnThreshold)
        {
            ent.Comp.ThirdWarnReached = true;

            DoPopup(ent, ent.Comp.ThirdWarnPopup, PopupType.LargeCaution);

            _stun.TryAddStunDuration(ent, ent.Comp.ThirdWarnStun);

        }
        else if (ent.Comp.Biomass > ent.Comp.ThirdWarnThreshold)
            ent.Comp.ThirdWarnReached = false;

        // point of no return
        if (ent.Comp.Biomass <= 0
            && !_absorbQuery.HasComp(ent))
            KillChangeling(ent);

    }

    #region Helper Methods
    private void UpdateBiomass(Entity<ChangelingBiomassComponent> ent)
    {
        var newBiomass = ent.Comp.Biomass -= ent.Comp.DrainAmount;
        ent.Comp.Biomass = Math.Clamp(newBiomass, 0, ent.Comp.MaxBiomass);

        _alerts.ShowAlert(ent.Owner, ent.Comp.AlertId);
    }

    public readonly ProtoId<DamageTypePrototype> Genetic = "Cellular";
    private void KillChangeling(Entity<ChangelingBiomassComponent> ent)
    {
        var genetic = _proto.Index(Genetic);

        if (!_mob.TryGetDeadThreshold(ent, out var totalDamage))
            return;

        EnsureComp<AbsorbedComponent>(ent);

        DoPopup(ent, ent.Comp.NoBiomassPopup, PopupType.LargeCaution);
    }

    protected virtual void DoCough(Entity<ChangelingBiomassComponent> ent)
    {
        // go to ChangelingBiomassSystem for the logic
    }

    private void DoPopup(Entity<ChangelingBiomassComponent> ent, LocId popup, PopupType popupType)
    {
        if (_net.IsClient)
            return;

        _popup.PopupEntity(Loc.GetString(popup), ent, ent, popupType);
    }

    #endregion

    #region Event Handlers
    private void OnRejuvenate(Entity<ChangelingBiomassComponent> ent, ref RejuvenateEvent args)
    {
        ent.Comp.Biomass = ent.Comp.MaxBiomass;
        Dirty(ent, ent.Comp);
    }

    #endregion
}
