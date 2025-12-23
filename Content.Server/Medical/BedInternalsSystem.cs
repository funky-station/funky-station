// SPDX-FileCopyrightText: 2025 otokonoko-dev <248204705+otokonoko-dev@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Components;
using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Medical.Components;
using Content.Server.Body.Systems;
using Content.Shared.Atmos.Components;
using Content.Shared.Body.Components;
using Content.Shared.Inventory;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Internals;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Audio;
using Robust.Shared.Utility;
using Robust.Server.Audio;
using Content.Shared.Medical;
using Robust.Server.GameObjects;

namespace Content.Server.Medical.Systems;

public sealed class BedInternalsSystem : EntitySystem
{
    [Dependency] private readonly InternalsSystem _internals = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly GasTankSystem _gasTank = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<BedInternalsComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
        SubscribeLocalEvent<BedInternalsComponent, EntInsertedIntoContainerMessage>(OnTankInserted);
        SubscribeLocalEvent<BedInternalsComponent, EntRemovedFromContainerMessage>(OnTankRemoved);
        SubscribeLocalEvent<BedInternalsComponent, StrappedEvent>(OnStrapped);
        SubscribeLocalEvent<BedInternalsComponent, UnstrappedEvent>(OnUnstrapped);
    }

    private void OnGetVerbs(EntityUid uid, BedInternalsComponent comp, GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        // Only show the internals toggle if someone is buckled into the bed.
        if (!TryComp<StrapComponent>(uid, out var strap) || strap.BuckledEntities.Count == 0)
            return;

        var verb = new InteractionVerb
        {
            Text = comp.Enabled ? "Disable Internals" : "Enable Internals",
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/dot.svg.192dpi.png")),
            Act = () => ToggleBedInternals(uid, comp)
        };

        args.Verbs.Add(verb);
    }

    private void OnTankInserted(EntityUid uid, BedInternalsComponent comp, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != comp.GasSlot)
            return;

        comp.CachedTank = args.Entity;

        UpdateTankVisual(uid, args.Entity);

        if (comp.Enabled && TryComp<StrapComponent>(uid, out var strap))
        {
            foreach (var patient in strap.BuckledEntities)
            {
                if (TryComp<InternalsComponent>(patient, out _))
                    ApplyInternalsToPatient(uid, comp, patient);
            }
        }
    }

    private void OnTankRemoved(EntityUid uid, BedInternalsComponent comp, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != comp.GasSlot)
            return;

        comp.CachedTank = null;

        UpdateTankVisual(uid, null);

        if (comp.Enabled)
        {
            comp.Enabled = false;
            if (TryComp<StrapComponent>(uid, out var strap))
            {
                foreach (var patient in strap.BuckledEntities.ToArray())
                {
                    RemoveInternalsFromPatient(uid, comp, patient);
                }
            }
        }
    }

    private void UpdateTankVisual(EntityUid bed, EntityUid? tank)
    {
        if (tank != null && TryComp<BedTankVisualComponent>(tank.Value, out var visual))
        {
            _appearance.SetData(bed, BedInternalsVisuals.TankVisual, visual.Visual);
        }
        else
        {
            _appearance.SetData(bed, BedInternalsVisuals.TankVisual, BedTankVisual.None);
        }
    }

    private void OnStrapped(EntityUid uid, BedInternalsComponent comp, StrappedEvent args)
    {
        if (!comp.Enabled || comp.CachedTank == null)
            return;

        var patient = args.Buckle.Owner;

        if (!TryComp<InternalsComponent>(patient, out var internals))
            return;

        ApplyInternalsToPatient(uid, comp, patient);
    }

    private void OnUnstrapped(EntityUid uid, BedInternalsComponent comp, UnstrappedEvent args)
    {
        var patient = args.Buckle.Owner;

        RemoveInternalsFromPatient(uid, comp, patient);

        // If the last patient has left the bed, reset internals to disabled.
        if (TryComp<StrapComponent>(uid, out var strap) && strap.BuckledEntities.Count == 0)
        {
            comp.Enabled = false;
        }
    }

    private void ToggleBedInternals(EntityUid uid, BedInternalsComponent comp)
    {
        if (!TryComp<StrapComponent>(uid, out var strap))
            return;

        // Don't allow enabling if there are no buckled patients.
        if (!comp.Enabled && strap.BuckledEntities.Count == 0)
            return;

        comp.Enabled = !comp.Enabled;

        if (comp.Enabled)
        {
            // Play a short internals enable sound only when there are buckled patients and a tank inserted.
            if (strap.BuckledEntities.Count > 0 && comp.CachedTank != null)
            {
                _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/internals.ogg"), uid);
            }

            foreach (var patient in strap.BuckledEntities)
            {
                if (TryComp<InternalsComponent>(patient, out _))
                    ApplyInternalsToPatient(uid, comp, patient);
            }
        }
        else
        {
            foreach (var patient in strap.BuckledEntities.ToArray())
            {
                RemoveInternalsFromPatient(uid, comp, patient);
            }
        }
    }

    private void ApplyInternalsToPatient(EntityUid bedUid, BedInternalsComponent comp, EntityUid patient)
    {
        if (!TryComp<InternalsComponent>(patient, out var internals))
            return;




        if (_inventory.TryGetSlotEntity(patient, "mask", out var existing))
        {
            if (existing != null && TryComp<BreathToolComponent>(existing, out _))
            {

                _internals.ConnectBreathTool((patient, internals), existing.Value);

                if (comp.CachedTank != null && TryComp<GasTankComponent>(comp.CachedTank.Value, out var tankComp))
                {

                    tankComp.User = patient;
                    _gasTank.ConnectToInternals((comp.CachedTank.Value, tankComp));


                    if (internals.GasTankEntity == null)
                    {
                        var ok = _internals.TryConnectTank((patient, internals), comp.CachedTank.Value);
                        if (ok)
                        {
                            tankComp.User = patient;
                            tankComp.CheckUser = false;
                            _gasTank.UpdateUserInterface((comp.CachedTank.Value, tankComp));
                        }
                    }
                }

                if (!_internals.AreInternalsWorking(internals))
                {
                    if (!_internals.AreInternalsWorking(patient))
                    {
                        RaiseLocalEvent(patient, new ToggleInternalsAlertEvent());
                    }

                }
                return;
            }


            if (existing != null)
            {
                if (_inventory.TryUnequip(patient, "mask", out var removed, silent: true, force: true))
                {
                    if (removed != null)
                        comp.StoredMasks[patient] = removed.Value;
                }
                else
                {
                    return;
                }
            }
        }


        EntityUid maskEnt;
        if (comp.TempMasks.TryGetValue(patient, out var temp) && temp != EntityUid.Invalid)
        {
            maskEnt = temp;
        }
        else
        {
            maskEnt = EntityManager.Spawn(comp.MaskPrototype);
            Transform(maskEnt).Coordinates = Transform(bedUid).Coordinates;
            comp.TempMasks[patient] = maskEnt;
        }



        var equipped = _inventory.TryEquip(patient, maskEnt, "mask", silent: true, force: true);
        if (!equipped)
            return;

        if (TryComp<BreathToolComponent>(maskEnt, out var breathComp))
        {
            breathComp.ConnectedInternalsEntity = patient;
        }

        _internals.ConnectBreathTool((patient, internals), maskEnt);


        if (comp.CachedTank != null && TryComp<GasTankComponent>(comp.CachedTank.Value, out var tankComp2))
        {
            tankComp2.User = patient;
            _gasTank.ConnectToInternals((comp.CachedTank.Value, tankComp2));

            if (internals.GasTankEntity == null)
            {
                var ok = _internals.TryConnectTank((patient, internals), comp.CachedTank.Value);
                if (ok)
                {
                    tankComp2.User = patient;
                    tankComp2.CheckUser = false;
                    _gasTank.UpdateUserInterface((comp.CachedTank.Value, tankComp2));
                }
            }
        }

        if (!_internals.AreInternalsWorking(patient))
        {
            RaiseLocalEvent(patient, new ToggleInternalsAlertEvent());
        }
    }


    private void RemoveInternalsFromPatient(EntityUid bedUid, BedInternalsComponent comp, EntityUid patient)
    {
        if (!TryComp<InternalsComponent>(patient, out var internals))
            return;

        if (!_internals.AreInternalsWorking(patient))
        {
            RaiseLocalEvent(patient, new ToggleInternalsAlertEvent());
        }


        if (comp.TempMasks.TryGetValue(patient, out var tempMask) && tempMask != EntityUid.Invalid)
        {
            _inventory.TryUnequip(patient, "mask", out var removed, silent: true, force: true);

            if (EntityManager.EntityExists(tempMask))
            {
                _internals.DisconnectBreathTool((patient, internals), tempMask);
            }

            if (removed == tempMask)
            {
                if (EntityManager.EntityExists(tempMask))
                    EntityManager.DeleteEntity(tempMask);
            }
            else if (removed != null && removed != tempMask)
            {

                if (EntityManager.EntityExists(tempMask))
                    EntityManager.DeleteEntity(tempMask);
            }

            comp.TempMasks.Remove(patient);
        }

        if (comp.StoredMasks.TryGetValue(patient, out var stored) && stored != EntityUid.Invalid)
        {
            _inventory.TryEquip(patient, stored, "mask", silent: true, force: true);
            comp.StoredMasks.Remove(patient);
        }
    }
}
