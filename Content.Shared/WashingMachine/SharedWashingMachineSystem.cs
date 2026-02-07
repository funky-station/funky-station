// SPDX-FileCopyrightText: 2025 Doctor-Cpu <77215380+Doctor-Cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Will-Oliver-Br <164823659+Will-Oliver-Br@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Destructible;
using Content.Shared.Interaction;
using Content.Shared.Power.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Traits.Assorted;
using Content.Shared.Verbs;
using Content.Shared.WashingMachine.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Shared.WashingMachine;

public abstract partial class SharedWashingMachineSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedEntityStorageSystem _storage = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private const float DamagePerSecond = 6.0f; // 30 damage over 5 seconds
    private static readonly SoundSpecifier HitSound = new SoundCollectionSpecifier("MetalThud");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WashingMachineComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<WashingMachineComponent, ComponentRemove>(OnRemoved);

        SubscribeLocalEvent<WashingMachineComponent, BreakageEventArgs>(OnBreak);
        SubscribeLocalEvent<WashingMachineComponent, StorageOpenAttemptEvent>(OnStorageOpenAttempt);

        SubscribeLocalEvent<WashingMachineComponent, ActivateInWorldEvent>(OnActivateInWorld, before: [typeof(SharedEntityStorageSystem)]);
        SubscribeLocalEvent<WashingMachineComponent, GetVerbsEvent<ActivationVerb>>(OnGetVerbs);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var blunt = _proto.Index<DamageTypePrototype>("Blunt");

        var query = EntityQueryEnumerator<WashingMachineActiveComponent, WashingMachineComponent>();
        while (query.MoveNext(out var uid, out var _, out var component))
        {
            if (component.WashingFinished <= _timing.CurTime)
            {
                if (_net.IsServer)
                    FinishWashing(uid, component);
                continue;
            }

            if (_net.IsServer)
            {
                SharedEntityStorageComponent? storage = null;

                if (_storage.ResolveStorage(uid, ref storage) && storage.Contents.ContainedEntities.Count > 0)
                {
                    var damage = new DamageSpecifier(blunt, DamagePerSecond * frameTime);

                    foreach (var item in storage.Contents.ContainedEntities)
                    {
                        _damageable.TryChangeDamage(item, damage, true);
                    }

                    if (_random.Prob(0.9f * frameTime))
                    {
                        _audio.PlayPvs(HitSound, uid);
                    }
                }
            }
        }
    }

    private void FinishWashing(EntityUid uid, WashingMachineComponent component)
    {
        RemComp<WashingMachineActiveComponent>(uid);

        component.WashingMachineState = WashingMachineState.Idle;
        DirtyField(uid, component, nameof(WashingMachineComponent.WashingMachineState));
        _appearance.SetData(uid, WashingMachineVisuals.State, component.WashingMachineState);

        HashSet<EntityUid> items = new();

        SharedEntityStorageComponent? entityStorageComp = null;
        if (_storage.ResolveStorage(uid, ref entityStorageComp))
            items = entityStorageComp.Contents.ContainedEntities.ToHashSet();

        component.WashingSoundStream = _audio.Stop(component.WashingSoundStream);

        _audio.PlayPvs(component.FinishedSound, uid);

        var machineEv = new WashingMachineFinishedWashingEvent(items);
        RaiseLocalEvent(uid, machineEv);

        var itemEv = new WashingMachineWashedEvent(uid, items);
        foreach (var item in items)
        {
            RaiseLocalEvent(item, itemEv);

            if (HasComp<StatusEffectsComponent>(item))
            {
                if (TryComp<MigraineComponent>(item, out var migraine))
                {
                    if (migraine.Duration != -1)
                    {
                        migraine.Duration = Math.Max(migraine.Duration, 30.0f);
                        migraine.BlurryMagnitude = Math.Max(migraine.BlurryMagnitude, 5.0f);
                        Dirty(item, migraine);
                    }
                }
                else
                {
                    migraine = AddComp<MigraineComponent>(item);
                    migraine.Duration = 30.0f;
                    migraine.BlurryMagnitude = 5.0f;
                    migraine.ApplySlowdown = true;
                    Dirty(item, migraine);
                }
            }
        }

        // update again incase forensics changed
        // such as dyeing
        UpdateForensics((uid, component), items);

        _storage.OpenStorage(uid);
    }

    private void OnInit(Entity<WashingMachineComponent> ent, ref ComponentInit args)
    {
        _appearance.SetData(ent.Owner, WashingMachineVisuals.State, ent.Comp.WashingMachineState);
    }

    private void OnRemoved(Entity<WashingMachineComponent> ent, ref ComponentRemove args)
    {
        _audio.Stop(ent.Comp.WashingSoundStream);
    }

    private void OnBreak(Entity<WashingMachineComponent> ent, ref BreakageEventArgs args)
    {
        ent.Comp.WashingMachineState = WashingMachineState.Broken;
        DirtyField(ent.Owner, ent.Comp, nameof(WashingMachineComponent.WashingMachineState));
        _appearance.SetData(ent.Owner, WashingMachineVisuals.State, ent.Comp.WashingMachineState);

        RemComp<WashingMachineActiveComponent>(ent.Owner);
    }

    private void OnStorageOpenAttempt(Entity<WashingMachineComponent> ent, ref StorageOpenAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        args.Cancelled = ent.Comp.WashingMachineState != WashingMachineState.Idle;
    }

    private void OnActivateInWorld(Entity<WashingMachineComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        if (!CanActivate(ent))
            return;

        args.Handled = true;
        Activate(ent);
    }

    private void OnGetVerbs(Entity<WashingMachineComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanInteract || !args.CanComplexInteract)
            return;

        if (!CanActivate(ent))
            return;

        var verb = new ActivationVerb()
        {
            Text = Loc.GetString("washing-machine-start"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/Spare/poweronoff.svg.192dpi.png")),
            Act = () => Activate(ent)
        };

        args.Verbs.Add(verb);
    }

    private bool CanActivate(Entity<WashingMachineComponent> ent)
    {
        if (ent.Comp.WashingMachineState != WashingMachineState.Idle)
            return false;

        if (!_power.IsPowered(ent.Owner))
            return false;

        if (_storage.IsOpen(ent.Owner))
            return false;

        return true;
    }

    private void Activate(Entity<WashingMachineComponent> ent)
    {
        ent.Comp.WashingFinished = _timing.CurTime + ent.Comp.WashingTime;
        DirtyField(ent.Owner, ent.Comp, nameof(WashingMachineComponent.WashingFinished));

        ent.Comp.WashingMachineState = WashingMachineState.Washing;
        DirtyField(ent.Owner, ent.Comp, nameof(WashingMachineComponent.WashingMachineState));
        _appearance.SetData(ent.Owner, WashingMachineVisuals.State, ent.Comp.WashingMachineState);

        EnsureComp<WashingMachineActiveComponent>(ent.Owner);

        HashSet<EntityUid> items = new();

        SharedEntityStorageComponent? entityStorageComp = null;
        if (_storage.ResolveStorage(ent.Owner, ref entityStorageComp))
            items = entityStorageComp.Contents.ContainedEntities.ToHashSet();

        if (_net.IsServer)
        {
            var audio = _audio.PlayPvs(ent.Comp.WashingSound, ent.Owner);
            ent.Comp.WashingSoundStream = audio?.Entity;
        }

        var machineEv = new WashingMachineStartedWashingEvent(items);
        RaiseLocalEvent(ent.Owner, machineEv);

        UpdateForensics(ent, items);

        var itemEv = new WashingMachineIsBeingWashed(ent.Owner, items);
        foreach (var item in items)
        {
            RaiseLocalEvent(item, itemEv);
        }
    }

    protected virtual void UpdateForensics(Entity<WashingMachineComponent> ent, HashSet<EntityUid> items)
    {
    }
}
