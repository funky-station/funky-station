// SPDX-FileCopyrightText: 2025 otokonoko-dev <248204705+otokonoko-dev@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Shared._Shitmed.Surgery;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Robust.Shared.Utility;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Shitmed.Surgery;

public sealed class OperatingTableLightSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MobThresholdSystem _mobThresholdSystem = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedPointLightSystem _pointLight = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OperatingTableLightComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<OperatingTableLightComponent, StrappedEvent>(OnStrapped);
        SubscribeLocalEvent<OperatingTableLightComponent, UnstrappedEvent>(OnUnstrapped);

        // Subscribe to health change events for live vitals updates
        SubscribeLocalEvent<DamageableComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnGetVerbs(EntityUid uid, OperatingTableLightComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        // Add verb to toggle light
        args.Verbs.Add(new AlternativeVerb
        {
            Text = comp.LightOn ? "Turn Off Light" : "Turn On Light",
            Act = () => ToggleLight(uid, comp),
            Priority = -1,  // Lower priority than ItemSlots eject (0) to avoid alt-click interference
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/light.svg.192dpi.png"))
        });
    }

    private void ToggleLight(EntityUid uid, OperatingTableLightComponent comp)
    {
        comp.LightOn = !comp.LightOn;
        _appearance.SetData(uid, OperatingTableVisuals.LightOn, comp.LightOn);
        _audio.PlayPvs("/Audio/Machines/button.ogg", uid);

        // Toggle the actual light emission
        if (TryComp<PointLightComponent>(uid, out var pointLight))
            _pointLight.SetEnabled(uid, comp.LightOn, pointLight);

        Dirty(uid, comp);
    }

    private void OnStrapped(EntityUid uid, OperatingTableLightComponent comp, ref StrappedEvent args)
    {
        var patient = args.Buckle.Owner;
        UpdateVitalsDisplay(uid, patient);

        // Start appropriate sound based on patient state
        if (TryComp<MobStateComponent>(patient, out var mobState))
        {
            if (_mobState.IsDead(patient, mobState))
            {
                PlayFlatline(uid, comp);
            }
            else
            {
                StartHeartbeat(uid, comp, patient);
            }
        }
    }

    private void OnUnstrapped(EntityUid uid, OperatingTableLightComponent comp, ref UnstrappedEvent args)
    {
        if (!TryComp<StrapComponent>(uid, out var strap) || strap.BuckledEntities.Count == 0)
        {
            _appearance.SetData(uid, OperatingTableVisuals.VitalsState, VitalsState.None);

            // Stop heartbeat and flatline when patient is unbuckled
            StopHeartbeat(uid, comp);
            StopFlatline(uid, comp);
        }
    }

    private void UpdateVitalsDisplay(EntityUid table, EntityUid patient)
    {
        if (!TryComp<MobStateComponent>(patient, out var mobState))
        {
            _appearance.SetData(table, OperatingTableVisuals.VitalsState, VitalsState.None);
            return;
        }

        VitalsState state;

        if (_mobState.IsDead(patient, mobState))
        {
            state = VitalsState.Dead;
        }
        else if (_mobState.IsCritical(patient, mobState))
        {
            state = VitalsState.Critical;
        }
        else if (TryComp<DamageableComponent>(patient, out var damageable))
        {
            var totalHealth = damageable.TotalDamage;
            var maxHealth = 100f;

            if (_mobThresholdSystem.TryGetThresholdForState(patient, MobState.Critical, out var critThreshold))
                maxHealth = critThreshold.Value.Float();

            var healthPercent = 1.0f - (totalHealth / maxHealth);

            if (healthPercent > 0.85f)
                state = VitalsState.Healthy;
            else if (healthPercent > 0.5f)
                state = VitalsState.Injured;
            else
                state = VitalsState.Critical;
        }
        else
        {
            state = VitalsState.Healthy;
        }

        _appearance.SetData(table, OperatingTableVisuals.VitalsState, state);
    }

    private SoundSpecifier? GetHeartbeatSoundForPatient(EntityUid uid, OperatingTableLightComponent comp, EntityUid patient)
    {
        if (!TryComp<MobStateComponent>(patient, out var mobState))
            return comp.HeartbeatHealthySound;

        if (_mobState.IsDead(patient, mobState))
            return null; // Dead patients don't get heartbeat

        if (_mobState.IsCritical(patient, mobState))
            return comp.HeartbeatCriticalSound;

        if (TryComp<DamageableComponent>(patient, out var damageable))
        {
            var totalHealth = damageable.TotalDamage.Float();
            var maxHealth = 100f;

            if (_mobThresholdSystem.TryGetThresholdForState(patient, MobState.Critical, out var critThreshold))
                maxHealth = critThreshold.Value.Float();

            var healthPercent = 1.0f - (totalHealth / maxHealth);

            // Match vitals display thresholds exactly
            if (healthPercent > 0.85f)
                return comp.HeartbeatHealthySound; // Healthy/Green
            else if (healthPercent > 0.5f)
                return comp.HeartbeatInjuredSound; // Injured/Orange
            else
                return comp.HeartbeatCriticalSound; // Critical/Red
        }

        return comp.HeartbeatHealthySound; // Default to healthy
    }

    private void UpdateHeartbeatPitch(EntityUid uid, OperatingTableLightComponent comp, EntityUid patient)
    {
        // Get what sound should be playing based on current health
        var targetSound = GetHeartbeatSoundForPatient(uid, comp, patient);

        // Only restart if the sound has changed (avoids unnecessary restarts on small damage changes)
        if (targetSound != comp.CurrentHeartbeatSound)
        {
            StartHeartbeat(uid, comp, patient);
        }
    }

    private void OnDamageChanged(EntityUid uid, DamageableComponent component, DamageChangedEvent args)
    {
        // uid is the patient who took damage
        // Check if this patient is buckled to an operating table
        if (!TryComp<BuckleComponent>(uid, out var buckle))
            return;

        if (buckle.BuckledTo == null)
            return;

        var table = buckle.BuckledTo.Value;

        // Check if the table has OperatingTableLight component (is an operating table)
        if (!TryComp<OperatingTableLightComponent>(table, out var tableComp))
            return;

        // Update the table's vitals display
        UpdateVitalsDisplay(table, uid);

        // Check if patient died or was revived
        if (TryComp<MobStateComponent>(uid, out var mobState))
        {
            if (_mobState.IsDead(uid, mobState))
            {
                PlayFlatline(table, tableComp);
            }
            else if (tableComp.HeartbeatStream == null)
            {
                // Patient was revived, restart heartbeat
                StopFlatline(table, tableComp);
                StartHeartbeat(table, tableComp, uid);
            }
            else
            {
                // Patient is alive and heartbeat is playing - update pitch
                UpdateHeartbeatPitch(table, tableComp, uid);
            }
        }
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        // args.Target is the patient whose state changed
        var uid = args.Target;

        // Check if this patient is buckled to an operating table
        if (!TryComp<BuckleComponent>(uid, out var buckle))
            return;

        if (buckle.BuckledTo == null)
            return;

        var table = buckle.BuckledTo.Value;

        // Check if the table has OperatingTableLight component
        if (!TryComp<OperatingTableLightComponent>(table, out var tableComp))
            return;

        // Update the table's vitals display
        UpdateVitalsDisplay(table, uid);

        // Check if patient died or was revived
        if (TryComp<MobStateComponent>(uid, out var mobState))
        {
            if (_mobState.IsDead(uid, mobState))
            {
                PlayFlatline(table, tableComp);
            }
            else if (tableComp.HeartbeatStream == null)
            {
                // Patient was revived, restart heartbeat
                StopFlatline(table, tableComp);
                StartHeartbeat(table, tableComp, uid);
            }
            else
            {
                // Patient is alive and heartbeat is playing - update pitch
                UpdateHeartbeatPitch(table, tableComp, uid);
            }
        }
    }

    private void StartHeartbeat(EntityUid uid, OperatingTableLightComponent comp, EntityUid patient)
    {
        // Stop any existing heartbeat first
        StopHeartbeat(uid, comp);

        // Get appropriate sound based on patient health
        var heartbeatSound = GetHeartbeatSoundForPatient(uid, comp, patient);
        if (heartbeatSound == null)
            return;

        // Play looping heartbeat sound (no pitch scaling)
        var stream = _audio.PlayPvs(
            heartbeatSound,
            uid,
            AudioParams.Default.WithLoop(true).WithVolume(-5f)
        );

        if (stream != null)
        {
            comp.HeartbeatStream = stream.Value.Entity;
            comp.CurrentHeartbeatSound = heartbeatSound;
        }
    }

    private void StopHeartbeat(EntityUid uid, OperatingTableLightComponent comp)
    {
        if (comp.HeartbeatStream != null)
        {
            _audio.Stop(comp.HeartbeatStream.Value);
            comp.HeartbeatStream = null;
            comp.CurrentHeartbeatSound = null;
        }
    }

    private void StopFlatline(EntityUid uid, OperatingTableLightComponent comp)
    {
        if (comp.FlatlineStream != null)
        {
            _audio.Stop(comp.FlatlineStream.Value);
            comp.FlatlineStream = null;
        }
    }

    private void PlayFlatline(EntityUid uid, OperatingTableLightComponent comp)
    {
        // Stop heartbeat
        StopHeartbeat(uid, comp);

        // Stop any existing flatline
        StopFlatline(uid, comp);

        // Play looping flatline sound
        if (comp.FlatlineSound != null)
        {
            var stream = _audio.PlayPvs(
                comp.FlatlineSound,
                uid,
                AudioParams.Default.WithLoop(true).WithVolume(-3f)
            );

            if (stream != null)
            {
                comp.FlatlineStream = stream.Value.Entity;
            }
        }
    }
}
