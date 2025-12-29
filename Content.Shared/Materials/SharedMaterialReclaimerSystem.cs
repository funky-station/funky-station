// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 Sailor <109166122+Equivocateur@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 Xcybitt <197952719+Xcybitt@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 willow <willowzeta632146@proton.me>
// SPDX-FileCopyrightText: 2025 wilowzeta <willowzeta632146@proton.me>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Shared.Administration.Logs;
using Content.Shared.Audio;
using Content.Shared.Body.Components;
using Content.Shared.Database;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Examine;
using Content.Shared.Mobs.Components;
using Content.Shared.Stacks;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Shared.Materials;

/// <summary>
/// Handles interactions and logic related to <see cref="MaterialReclaimerComponent"/>,
/// and <see cref="CollideMaterialReclaimerComponent"/>.
/// </summary>
public abstract class SharedMaterialReclaimerSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedAmbientSoundSystem AmbientSound = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] protected readonly SharedContainerSystem Container = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private readonly EmagSystem _emag = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<MaterialReclaimerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MaterialReclaimerComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<MaterialReclaimerComponent, GotEmaggedEvent>(OnEmagged);
        SubscribeLocalEvent<MaterialReclaimerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CollideMaterialReclaimerComponent, StartCollideEvent>(OnCollide);
    }

    private void OnMapInit(EntityUid uid, MaterialReclaimerComponent component, MapInitEvent args)
    {
        component.NextSound = Timing.CurTime;
    }

    private void OnShutdown(EntityUid uid, MaterialReclaimerComponent component, ComponentShutdown args)
    {
        _audio.Stop(component.Stream);
    }

    private void OnExamined(EntityUid uid, MaterialReclaimerComponent component, ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("recycler-count-items", ("items", component.ItemsProcessed)));
    }

    private void OnEmagged(EntityUid uid, MaterialReclaimerComponent component, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        if (_emag.CheckFlag(uid, EmagType.Interaction))
            return;

        args.Handled = true;
    }

    private void OnCollide(EntityUid uid, CollideMaterialReclaimerComponent component, ref StartCollideEvent args)
    {
        if (args.OurFixtureId != component.FixtureId)
            return;
        if (!TryComp<MaterialReclaimerComponent>(uid, out var reclaimer))
            return;
        TryQueueItem(uid, args.OtherEntity, reclaimer);
    }

    /// <summary>
    /// Tries to queue an item for processing via a <see cref="MaterialReclaimerComponent"/>.
    /// </summary>
    public bool TryQueueItem(EntityUid uid, EntityUid item, MaterialReclaimerComponent? component = null, EntityUid? user = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        // Duplication prevention: check if item is already in the container
        var queueContainer = Container.EnsureContainer<Container>(uid, MaterialReclaimerComponent.QueueContainerId);
        if (queueContainer.Contains(item))
            return false;

        if (!CanStart(uid, component))
            return false;

        if (HasComp<MobStateComponent>(item) && !CanGib(uid, item, component)) // whitelist? We be gibbing, boy!
            return false;

        if (_whitelistSystem.IsWhitelistFail(component.Whitelist, item) ||
            _whitelistSystem.IsWhitelistPass(component.Blacklist, item))
            return false;

        if (Container.TryGetContainingContainer((item, null, null), out _) && !Container.TryRemoveFromContainer(item))
            return false;

        if (user != null)
        {
            _adminLog.Add(LogType.Action,
                LogImpact.High,
                $"{ToPrettyString(user.Value):player} destroyed {ToPrettyString(item)} in the material reclaimer, {ToPrettyString(uid)}");
        }

        if (Timing.CurTime > component.NextSound)
        {
            component.Stream = _audio.PlayPredicted(component.Sound, uid, user)?.Entity;
            component.NextSound = Timing.CurTime + component.SoundCooldown;
        }

        var reclaimedEvent = new GotReclaimedEvent(Transform(uid).Coordinates);
        RaiseLocalEvent(item, ref reclaimedEvent);

        Container.Insert(item, queueContainer);
        component.ProcessingQueue.Add(item);
        Dirty(uid, component);

        return true;
    }


    /// <summary>
    /// Spawns the materials and chemicals associated
    /// with an entity. Also deletes the item.
    /// </summary>
    public virtual void Reclaim(EntityUid uid,
        EntityUid item,
        float completion = 1f,
        MaterialReclaimerComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.ItemsProcessed++;
        if (component.CutOffSound)
        {
            _audio.Stop(component.Stream);
        }

        Dirty(uid, component);
    }

    /// <summary>
    /// Sets the Enabled field on the reclaimer.
    /// </summary>
    public bool SetReclaimerEnabled(EntityUid uid, bool enabled, MaterialReclaimerComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return true;

        if (component.Broken && enabled)
            return false;

        component.Enabled = enabled;
        AmbientSound.SetAmbience(uid, enabled && component.Powered);
        Dirty(uid, component);

        return true;
    }

    /// <summary>
    /// Whether or not the specified reclaimer can currently
    /// begin reclaiming another entity.
    /// </summary>
    public bool CanStart(EntityUid uid, MaterialReclaimerComponent component)
    {
        return component.Powered && component.Enabled && !component.Broken;
    }

    /// <summary>
    /// Whether or not the reclaimer satisfies the conditions
    /// allowing it to gib/reclaim a living creature.
    /// </summary>
    public bool CanGib(EntityUid uid, EntityUid victim, MaterialReclaimerComponent component)
    {
        return component.Powered &&
               component.Enabled &&
               !component.Broken &&
               HasComp<BodyComponent>(victim) &&
               _emag.CheckFlag(uid, EmagType.Interaction);
    }

    /// <summary>
    /// Gets the duration of processing a specified entity.
    /// Processing is calculated from the sum of the materials within the entity.
    /// It does not regard the chemicals within it.
    /// </summary>
    public TimeSpan GetReclaimingDuration(EntityUid reclaimer,
        EntityUid item,
        MaterialReclaimerComponent? reclaimerComponent = null,
        PhysicalCompositionComponent? compositionComponent = null)
    {
        if (!Resolve(reclaimer, ref reclaimerComponent))
            return TimeSpan.Zero;

        if (!reclaimerComponent.ScaleProcessSpeed ||
            !Resolve(item, ref compositionComponent, false))
            return reclaimerComponent.MinimumProcessDuration;

        var materialSum = compositionComponent.MaterialComposition.Values.Sum();
        materialSum *= CompOrNull<StackComponent>(item)?.Count ?? 1;
        var duration = TimeSpan.FromSeconds(materialSum / reclaimerComponent.MaterialProcessRate);
        if (duration < reclaimerComponent.MinimumProcessDuration)
            duration = reclaimerComponent.MinimumProcessDuration;
        return duration;
    }

    /// <summary>
    /// Legacy method name for backwards compatibility.
    /// </summary>
    [Obsolete("Use TryQueueItem instead")]
    public bool TryStartProcessItem(EntityUid uid, EntityUid item, MaterialReclaimerComponent? component = null, EntityUid? user = null)
    {
        return TryQueueItem(uid, item, component, user);
    }
}

[ByRefEvent]
public record struct GotReclaimedEvent(EntityCoordinates ReclaimerCoordinates);
