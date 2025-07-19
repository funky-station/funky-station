// SPDX-FileCopyrightText: 2025 GreyMaria <mariomister541@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Access.Components; // funkystation
using Content.Server.CartridgeLoader.Cartridges; // funkystation
using Content.Server.Forensics;
using Content.Shared._DV.NanoChat; // funkystation
using Content.Shared.Access.Components; // funkystation
using Content.Shared.Access.Systems; // funkystation
using Content.Shared.CartridgeLoader.Cartridges; // funkystation
using Content.Shared.Cloning.Events;
using Content.Shared.Clothing.Components;
using Content.Shared.Containers.ItemSlots; // funkystation
using Content.Shared.FixedPoint;
using Content.Shared.Labels.Components;
using Content.Shared.Labels.EntitySystems;
using Content.Shared.Paper;
using Content.Shared.PDA; // funkystation
using Content.Shared.Stacks;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;
using System.Linq; // funkystation

namespace Content.Server.Cloning;

/// <summary>
///     The part of item cloning responsible for copying over important components.
///     This is used for <see cref="CopyItem"/>.
///     Anything not copied over here gets reverted to the values the item had in its prototype.
/// </summary>
/// <remarks>
///     This method of copying items is of course not perfect as we cannot clone every single component, which would be pretty much impossible with our ECS.
///     We only consider the most important components so the paradox clone gets similar equipment.
///     This method of using subscriptions was chosen to make it easy for forks to add their own custom components that need to be copied.
/// </remarks>
public sealed partial class CloningSystem : EntitySystem
{
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly SharedLabelSystem _label = default!;
    [Dependency] private readonly ForensicsSystem _forensics = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!; // funkystation
    [Dependency] private readonly SharedIdCardSystem _idCard = default!; // funkystation
    [Dependency] private readonly SharedNanoChatSystem _nano = default!; // funkystation

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StackComponent, CloningItemEvent>(OnCloneStack);
        SubscribeLocalEvent<LabelComponent, CloningItemEvent>(OnCloneLabel);
        SubscribeLocalEvent<PaperComponent, CloningItemEvent>(OnClonePaper);
        SubscribeLocalEvent<ForensicsComponent, CloningItemEvent>(OnCloneForensics);
        SubscribeLocalEvent<StoreComponent, CloningItemEvent>(OnCloneStore);
        SubscribeLocalEvent<IdCardComponent, CloningItemEvent>(OnCloneIdCard); // funkystation
        SubscribeLocalEvent<AccessComponent, CloningItemEvent>(OnCloneAccess); // funkystation
        SubscribeLocalEvent<PdaComponent, CloningItemEvent>(OnClonePda); // funkystation
        SubscribeLocalEvent<NanoChatCardComponent, CloningItemEvent>(OnCloneNanochat); // funkystation
    }

    private void OnCloneStack(Entity<StackComponent> ent, ref CloningItemEvent args)
    {
        // if the clone is a stack as well, adjust the count of the copy
        if (TryComp<StackComponent>(args.CloneUid, out var cloneStackComp))
            _stack.SetCount(args.CloneUid, ent.Comp.Count, cloneStackComp);
    }

    private void OnCloneLabel(Entity<LabelComponent> ent, ref CloningItemEvent args)
    {
        // copy the label
        _label.Label(args.CloneUid, ent.Comp.CurrentLabel);
    }

    private void OnClonePaper(Entity<PaperComponent> ent, ref CloningItemEvent args)
    {
        // copy the text and any stamps
        if (TryComp<PaperComponent>(args.CloneUid, out var clonePaperComp))
        {
            _paper.SetContent((args.CloneUid, clonePaperComp), ent.Comp.Content);
            _paper.CopyStamps(ent.AsNullable(), (args.CloneUid, clonePaperComp));
        }
    }

    private void OnCloneForensics(Entity<ForensicsComponent> ent, ref CloningItemEvent args)
    {
        // copy any forensics to the cloned item
        _forensics.CopyForensicsFrom(ent.Comp, args.CloneUid);
    }

    private void OnCloneStore(Entity<StoreComponent> ent, ref CloningItemEvent args)
    {
        // copy the current amount of currency in the store
        // at the moment this takes care of uplink implants and the portable nukie uplinks
        // turning a copied pda into an uplink will need some refactoring first
        if (TryComp<StoreComponent>(args.CloneUid, out var cloneStoreComp))
        {
            cloneStoreComp.Balance = new Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2>(ent.Comp.Balance);
        }
    }

    // funkystation
    private void OnCloneIdCard(Entity<IdCardComponent> ent, ref CloningItemEvent args)
    {
        // id cards may be modified; ensure these modifications match
        if (TryComp<IdCardComponent>(args.CloneUid, out var cloneIdCard))
        {
            cloneIdCard.JobIcon = ent.Comp.JobIcon;
            _idCard.TryChangeFullName(args.CloneUid, ent.Comp.FullName);
            _idCard.TryChangeJobTitle(args.CloneUid, ent.Comp.LocalizedJobTitle);
        }
    }

    // funkystation
    private void OnCloneAccess(Entity<AccessComponent> ent, ref CloningItemEvent args)
    {
        // access components may be modified; ensure these modifications match
        if (TryComp<AccessComponent>(args.CloneUid, out var cloneAccess))
        {
            cloneAccess.Tags.Clear();
            cloneAccess.Tags.UnionWith(ent.Comp.Tags);
            RemComp<PresetIdCardComponent>(args.CloneUid); // we just set accesses, no need to initialize them now
        }
    }

    // funkystation
    private void OnClonePda(Entity<PdaComponent> ent, ref CloningItemEvent args)
    {
        // if we cloned a whole PDA, we need to explicitly copy its contents too; welcome to hell
        // actually, REALLY GENUINELY welcome to hell; we have to copy some installed program data from here too
        if (TryComp<PdaComponent>(args.CloneUid, out var clonePda))
        {
            clonePda.OwnerName = ent.Comp.OwnerName;
            clonePda.PdaOwner = ent.Comp.PdaOwner;

            // ID, pen, pAI, and cartridge slots
            if (TryComp<ItemSlotsComponent>(args.CloneUid, out var clonePdaSlots) && TryComp<ItemSlotsComponent>(ent, out var oldPdaSlots))
            {
                foreach (var slot in clonePdaSlots.Slots)
                {
                    if (_itemSlots.TryGetSlot(ent, slot.Key, out var oldSlot) && _itemSlots.TryGetSlot(args.CloneUid, slot.Key, out var newSlot) && newSlot.ContainerSlot != null)
                    {
                        var trash = _itemSlots.GetItemOrNull(args.CloneUid, slot.Key);
                        if (trash != null)
                            _container.Remove(trash.Value, newSlot.ContainerSlot);

                        if (oldSlot.Item != null)
                        {
                            var newItem = CopyItem(oldSlot.Item.Value, Transform(ent).Coordinates);
                            if (newItem != null)
                                _container.Insert(newItem.Value, newSlot.ContainerSlot);
                        }

                        QueueDel(trash);
                    }
                }
            }

            // installed programs
            if (_container.TryGetContainer(args.CloneUid, "program-container", out var newPrograms) && _container.TryGetContainer(ent, "program-container", out var oldPrograms))
            {
                // there's GOT to be a BETTER WAY
                foreach (var progId in oldPrograms.ContainedEntities)
                {
                    if (TryComp<NanoTaskCartridgeComponent>(progId, out var oldNanotask))
                    {
                        if (!TryComp<NanoTaskCartridgeComponent>(newPrograms.ContainedEntities.Where(id => HasComp<NanoTaskCartridgeComponent>(id)).First(), out var newNanotask))
                            continue;
                        newNanotask.Counter = oldNanotask.Counter;
                        foreach (var task in oldNanotask.Tasks)
                            newNanotask.Tasks.Add(task);
                        continue;
                    }
                    if (TryComp<NotekeeperCartridgeComponent>(progId, out var oldNotes))
                    {
                        if (!TryComp<NotekeeperCartridgeComponent>(newPrograms.ContainedEntities.Where(id => HasComp<NotekeeperCartridgeComponent>(id)).First(), out var newNotes))
                            continue;
                        foreach (var note in oldNotes.Notes)
                            newNotes.Notes.Add(note);
                        continue;
                    }
                }
            }
        }
    }

    // funkystation
    private void OnCloneNanochat(Entity<NanoChatCardComponent> ent, ref CloningItemEvent args)
    {
        // copy the NanoChat ID number and unlist the card to properly clone sent messages without revealing the Paradox Clone immediately in nanochat listing
        if (ent.Comp.Number != null)
            _nano.SetNumber(args.CloneUid, ent.Comp.Number.Value);
        _nano.SetListNumber(args.CloneUid, false);

        var oldRecipients = ent.Comp.Recipients;
        var oldMessages = ent.Comp.Messages;
        if (oldRecipients != null && oldMessages != null && TryComp<NanoChatCardComponent>(args.CloneUid, out var newCard))
        {
            foreach (var recipient in oldRecipients)
            {
                _nano.EnsureRecipientExists(args.CloneUid, recipient.Key, recipient.Value);
                var toCopy = oldMessages.Where(m => m.Key == recipient.Key).First().Value;
                foreach (var msg in toCopy)
                    _nano.AddMessage(args.CloneUid, recipient.Key, msg);
            }
        }
    }
}
