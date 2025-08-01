// SPDX-FileCopyrightText: 2025 vectorassembly <vectorassembly@icloud.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Content.Shared.Damage.Components;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Map;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared.Damage.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class SensitiveHearingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;

    private const float PENETRATION_VOLUME = 100.0f;
    public override void Initialize()
    {
        SubscribeLocalEvent<SensitiveHearingComponent, ComponentRemove>(OnCompRemove);
        SubscribeLocalEvent<ScreamActionEvent>(OnScreamAction);
        base.Initialize();
    }

    private void OnScreamAction(ScreamActionEvent ev)
    {
        if (!TryComp<TransformComponent>(ev.Performer, out var xform))
            return;
        BlastRadius(10, 2, _transformSystem.GetMapCoordinates(xform));
    }

    private void OnCompRemove(EntityUid uid, SensitiveHearingComponent comp, ComponentRemove args)
    {
        _alertsSystem.ClearAlertCategory(uid, comp.HearingAlertProtoCategory);
    }

    public void BlastRadius(float amount, float radius, MapCoordinates coords)
    {
        var blastEntities = _lookupSystem.GetEntitiesInRange(coords, radius);
        foreach (var entity in blastEntities)
        {
            // skips an iteration if entity does not have sensitive hearing or does not have a valid position in the world.
            if (!TryComp<SensitiveHearingComponent>(entity, out var hearing) || !HasComp<TransformComponent>(entity))
                continue;

            var entCoords =  _transformSystem.GetMapCoordinates(entity);

            //pythagoras theorem
            var distance = Math.Sqrt(Math.Pow(entCoords.X - coords.X, 2.0) + Math.Pow(entCoords.Y - coords.Y, 2.0));

            if (amount < PENETRATION_VOLUME)
            {
                //lowkey no clue how to use a predicate here. this works
                if (!_examine.InRangeUnOccluded(coords, entCoords, radius, predicate: _ => false))
                    continue;
            }
            else
                amount /= 3.0f;

            //show pain message when a certain damage threshold is passed, in or case this threshold is 50.0f.
            if (hearing.DamageAmount >= hearing.WarningThreshold)
            {
                //I don't like using var, feel free to use intellisense.
                //get user's ISession to show the message locally, didn't test this out yet.
                var iSession = GetEntityICommonSession(entity);
                if (iSession == null)
                    return;

                // Rupture the eardrums once a certain threshold is passed.
                if (hearing.DamageAmount >= hearing.DeafnessThreshold && !hearing.RuptureFlag)
                {
                    hearing.RuptureFlag = true;
                    _popupSystem.PopupEntity(Loc.GetString("damage-sensitive-hearing-eardrums-rupture"), entity, iSession, PopupType.LargeCaution);
                }

                //Alert the user when they have hearing damage but their eardrums are not ruptured yet. This goes below critical threshold check to avoid showing two messages at the same time.
                if (!hearing.RuptureFlag)
                    _popupSystem.PopupEntity(Loc.GetString("damage-sensitive-hearing-eardrums-tremble"), entity, iSession, PopupType.MediumCaution);
            }

            if (!hearing.IsDeaf)
                hearing.DamageAmount += GetBlastDamageModifier(entity) * CalculateFalloff(amount, radius, distance) * hearing.DamageModifier;
            Dirty(entity, hearing);
        }

    }

    private ICommonSession? GetEntityICommonSession(EntityUid entity)
    {
        if (!TryComp<MindContainerComponent>(entity, out var mindContainer) || !mindContainer.HasMind)
            return null;
        return CompOrNull<MindComponent>(mindContainer.Mind)?.Session;
    }

    private float CalculateFalloff(float maxDamage, float maxDistance, double sample)
    {
        // NOTE: Using linear formula because it deals better damage.
        double x = sample / maxDistance;
        //no clue how safe an explicit cast here is
        return (float) (-x*x+1) * maxDamage;
        // return (float) Math.Pow(x - 1, 2) * maxDamage;
        //-x^{2}+1
        // return (float) Math.Pow((1 - (1 / maxDistance) * sample), 2) * maxDamage;
    }


    private float GetBlastDamageModifier(EntityUid target)
    {
        var damageModifier = 1.0f;
        foreach (var slot in new[] {"head", "ears"})
        {
            _inventorySystem.TryGetSlotEntity(target, slot, out var item);
            if (!TryComp<LoudNoiseSuppressorComponent>(item, out var loudNoiseComponent))
                continue;

            // Invert the float value, so the damage modifier will be equal to 0.0f if suppression modifier is 1.0f
            damageModifier *= 1.0f - loudNoiseComponent.SuppressionModifier;
        }

        return damageModifier;
    }

    private void UpdateAlerts(SensitiveHearingComponent hearing, EntityUid entity)
    {
        if (hearing.DamageAmount < hearing.WarningThreshold)
        {
            _alertsSystem.ClearAlertCategory(entity, hearing.HearingAlertProtoCategory);
            return;
        }

        if (hearing.IsDeaf)
        {
            _alertsSystem.ShowAlert(entity, hearing.HearingDeafAlertProtoId);
            // _alertsSystem.ClearAlert(entity, hearing.HearingWarningAlertProtoId);
        }
        else
        {
            if (hearing.IsDeaf)
                return;
            _alertsSystem.ClearAlert(entity, hearing.HearingDeafAlertProtoId);
            var severity = Math.Max(0, hearing.DamageAmount - hearing.WarningThreshold) / (hearing.DeafnessThreshold - hearing.WarningThreshold) * 6.0f;
            short shortSeverity = (short) Math.Max(1, Math.Min(severity, 4));
            _alertsSystem.ShowAlert(entity, hearing.HearingWarningAlertProtoId, shortSeverity);
        }

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SensitiveHearingComponent>();
        while (query.MoveNext(out var uid, out var hearing))
        {
            if (_timing.CurTime >= hearing.NextThresholdUpdateTime)
            {
                hearing.NextThresholdUpdateTime = _timing.CurTime + hearing.ThresholdUpdateRate;
                UpdateAlerts(hearing, uid);
            }

            if (_timing.CurTime >= hearing.NextSelfHeal && !hearing.IsDeaf)
            {
                hearing.NextSelfHeal = _timing.CurTime + hearing.SelfHealRate;
                SelfHeal(hearing);
            }
        }
    }

    private void SelfHeal(SensitiveHearingComponent hearing)
    {
        hearing.DamageAmount -= hearing.SelfHealAmount;
    }

    public void HealHearingLoss(SensitiveHearingComponent hearing, EntityUid uid)
    {
        if (hearing.IsDeaf)
        {
            _alertsSystem.ClearAlertCategory(uid, hearing.HearingAlertProtoCategory);
            hearing.DamageAmount = hearing.DeafnessThreshold - hearing.DeafnessThreshold / 2;
        }
    }
}

