using Robust.Shared.Timing;
using Robust.Server.GameObjects;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Antag;
using Content.Shared.Changeling;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.Roles;
using Content.Shared.Verbs;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Server.Body.Systems;
using Content.Shared.Store.Components;
using Robust.Shared.Random;
using Content.Shared.Bed.Sleep;
using Content.Shared.Popups;
using Content.Shared.Jittering;

namespace Content.Server.Changeling;

public sealed partial class ChangelingInfectionSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;

    [Dependency] private readonly SharedJitteringSystem _jitterSystem = default!;

    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    [Dependency] private readonly AntagSelectionSystem _antag = default!;


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        foreach (var comp in EntityManager.EntityQuery<ChangelingInfectionComponent>())
        {
            var uid = comp.Owner;

            if (!comp.DoThings) {
                comp.FirstSymptoms = _timing.CurTime + TimeSpan.FromSeconds(comp.FirstSymptomsDelay);

                comp.KnockedOut = _timing.CurTime + TimeSpan.FromSeconds(comp.KnockedOutDelay);

                comp.FullyInfected = _timing.CurTime + TimeSpan.FromSeconds(comp.FullyInfectedDelay);
            }

            if (_timing.CurTime > comp.FirstSymptoms)
            {
                comp.CurrentState = ChangelingInfectionComponent.InfectionState.FirstSymptoms;
                comp.FirstSymptoms = _timing.CurTime + TimeSpan.FromHours(24f); // Don't fire again
            }
            else if (_timing.CurTime > comp.KnockedOut)
            {
                comp.CurrentState = ChangelingInfectionComponent.InfectionState.KnockedOut;
                comp.KnockedOut = _timing.CurTime + TimeSpan.FromHours(24f); // Hacky solution 2: Electric Boogaloo
            }
            else if (_timing.CurTime > comp.FullyInfected)
            {
                comp.CurrentState = ChangelingInfectionComponent.InfectionState.FullyInfected;
                comp.FullyInfected = _timing.CurTime + TimeSpan.FromHours(24f); // Ehhhhh nobody's gonna see this the component is getting removed in a tick anyway!
            }

            if (_timing.CurTime < comp.EffectsTimer)
                continue;

            comp.EffectsTimer = _timing.CurTime + TimeSpan.FromSeconds(comp.EffectsTimerDelay);

            if (comp.DoThings)
                DoEffects(uid, comp);

            comp.DoThings = true; // First tick over, setup's complete, we can do the stuff now

        }
    }
    public void DoEffects(EntityUid uid, ChangelingInfectionComponent comp)
    {
        // Switch statement to determine which stage of infection we're in

        switch (comp.CurrentState)
        {
            case ChangelingInfectionComponent.InfectionState.FirstSymptoms:

                break;
            case ChangelingInfectionComponent.InfectionState.KnockedOut:
                // Add forced knocked out component
                if (!EntityManager.HasComponent<ForcedSleepingComponent>(uid)) {
                    EntityManager.AddComponent<ForcedSleepingComponent>(uid);
                    _popupSystem.PopupEntity(Loc.GetString("changeling-convert-eeped"), uid, uid);
                    break;
                }
                if (_random.Prob(comp.ScarySymptomChance)) {
                    _jitterSystem.DoJitter(uid, TimeSpan.FromSeconds(5f), false, 10.0f, 4.0f);
                    _popupSystem.PopupEntity(Loc.GetString("changeling-convert-eeped-shake"), uid, uid);
                    break;
                }
                _popupSystem.PopupEntity(Loc.GetString(_random.Pick(comp.EepyMessages)), uid, uid);
                break;
            case ChangelingInfectionComponent.InfectionState.FullyInfected:
                // This will totally have no adverse effects whatsoever!
                if (!HasComp<MindContainerComponent>(uid) || !TryComp<ActorComponent>(uid, out var targetActor))
                    return;
                 _antag.ForceMakeAntag<ChangelingRuleComponent>(targetActor.PlayerSession, "Changeling");

                EntityManager.RemoveComponent<ChangelingInfectionComponent>(uid);

                _popupSystem.PopupEntity(Loc.GetString("changeling-convert-skillissue"), uid, uid);
                if (EntityManager.HasComponent<ForcedSleepingComponent>(uid))
                    EntityManager.RemoveComponent<ForcedSleepingComponent>(uid);

                break;
            case ChangelingInfectionComponent.InfectionState.None:
                break;
        }
    }
}

