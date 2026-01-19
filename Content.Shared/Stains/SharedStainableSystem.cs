// SPDX-FileCopyrightText: 2025 Doctor-Cpu <77215380+Doctor-Cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Will-Oliver-Br <164823659+Will-Oliver-Br@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Fluids;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Slippery;
using Content.Shared.WashingMachine.Events;
using Robust.Shared.Containers; // Gaby
using Content.Shared.Stains.Components; // Gaby
using Content.Shared.Verbs; // Gaby
using Content.Shared.DoAfter; // Gaby
using Content.Shared.Popups; // Gaby
using Content.Shared.FixedPoint;
using Robust.Shared.Utility; // Gaby

namespace Content.Shared.Stains;

public abstract partial class SharedStainableSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] protected readonly SharedSolutionContainerSystem Solution = default!;
    [Dependency] private readonly InventorySystem _inventory = default!; // Gaby
    [Dependency] private readonly SharedContainerSystem _container = default!; // Gaby
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!; // Gaby
    [Dependency] private readonly SharedPuddleSystem _puddle = default!; // Gaby
    [Dependency] private readonly SharedPopupSystem _popup = default!; // Gaby

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StainableComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<StainableComponent, InventoryRelayedEvent<SlippedEvent>>(OnSlipped);
        SubscribeLocalEvent<StainableComponent, InventoryRelayedEvent<SpilledOnEvent>>(OnSpilledOn);

        SubscribeLocalEvent<StainableComponent, WashingMachineIsBeingWashed>(OnWashed);

        SubscribeLocalEvent<StainableComponent, GetVerbsEvent<Verb>>(AddWringVerb); // Gaby
        SubscribeLocalEvent<StainableComponent, WringStainDoAfterEvent>(OnWringDoAfter); // Gaby
    }

    private void OnMapInit(Entity<StainableComponent> ent, ref MapInitEvent args)
    {
        if (string.IsNullOrEmpty(ent.Comp.SolutionId))
            return;

        if (!Solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out var solution))
            return;

        solution.Value.Comp.Solution.CanReact = false;
        UpdateVisuals(ent);
    }

    private void OnSlipped(Entity<StainableComponent> ent, ref InventoryRelayedEvent<SlippedEvent> args)
    {
        if (IsStainBlocked(ent)) // Gaby
            return;

        if (!Solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out var target))
            return;

        var ev = new GetStainableSolutionEvent(ent.Owner);
        RaiseLocalEvent(args.Args.Slipper, ev);

        if (!ev.Handled || ev.Solution == null)
            return;

        // Gaby - Pass target.Value because TryGetSolution returns a nullable struct
        TransferStain(target.Value, ev.Solution, ent.Comp.StainVolume);

        UpdateVisuals(ent);
        StainForensics(ent, target.Value);

        DirtyOwnerAppearance(ent.Owner); // Gaby
    }

    private void OnSpilledOn(Entity<StainableComponent> ent, ref InventoryRelayedEvent<SpilledOnEvent> args)
    {
        if (IsStainBlocked(ent)) // Gaby
            return;

        if (!Solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out var target))
            return;

        // Gaby - Pass target.Value because TryGetSolution returns a nullable struct
        TransferStain(target.Value, args.Args.Solution, ent.Comp.StainVolume);

        UpdateVisuals(ent);
        StainForensics(ent, target.Value);

        DirtyOwnerAppearance(ent.Owner); // Gaby
    }

    private void TransferStain(Entity<SolutionComponent> target, Solution source, FixedPoint2 amount)
    {
        // Call SplitSolution on the solution object directly, not the system.
        var taken = source.SplitSolution(amount);

        // Filter out water
        // Iterate backwards to remove items while iterating
        for (var i = taken.Contents.Count - 1; i >= 0; i--)
        {
            if (taken.Contents[i].Reagent.Prototype == "Water")
            {
                taken.RemoveReagent(taken.Contents[i].Reagent, taken.Contents[i].Quantity);
            }
        }

        // Transfer the remaining reagents to the target
        if (taken.Volume > 0)
        {
            Solution.TryAddSolution(target, taken);
        }
    }

    private bool IsStainBlocked(Entity<StainableComponent> item) // Gaby
    {
        if (!_container.TryGetContainingContainer(item.Owner, out var container))
            return false;
        var wearer = container.Owner;

        if (!TryComp<InventoryComponent>(wearer, out var inventory))
            return false;

        // Get the flags of the slot the item is currently in
        if (!_inventory.TryGetSlot(wearer, container.ID, out var slotDef, inventory))
            return false;

        var inSlotFlag = slotDef.SlotFlags;

        foreach (var slot in inventory.Slots)
        {
            if (!_inventory.TryGetSlotEntity(wearer, slot.Name, out var slotItem, inventory))
                continue;

            if (TryComp<StainBlockerComponent>(slotItem, out var blocker))
            {
                if ((blocker.Slots & inSlotFlag) != 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void OnWashed(Entity<StainableComponent> ent, ref WashingMachineIsBeingWashed args)
    {
        if (!Solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out var solution))
            return;

        WashingForensics(ent, solution.Value, args.WashingMachine);
        Solution.RemoveAllSolution(solution.Value);
        UpdateVisuals(ent);
    }

    protected virtual void StainForensics(Entity<StainableComponent> ent, Entity<SolutionComponent> solution)
    {
    }

    protected virtual void WashingForensics(Entity<StainableComponent> ent, Entity<SolutionComponent> solution, EntityUid washingMachine)
    {
    }

    private void UpdateVisuals(Entity<StainableComponent> ent)
    {
        _item.VisualsChanged(ent.Owner);

        // there isnt a value to parse as its calculated on every change
        // so just do a blanket update and calculate on the client
        if (TryComp<AppearanceComponent>(ent.Owner, out var appearance))
        {
            _appearance.QueueUpdate(ent.Owner, appearance);

            if (TryComp<MetaDataComponent>(ent.Owner, out var meta) && meta.EntityLifeStage < EntityLifeStage.Terminating)
                Dirty(ent.Owner, appearance);
        }
    }

    protected virtual void DirtyOwnerAppearance(EntityUid owner) // Gaby
    {
    }

    private void AddWringVerb(Entity<StainableComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (args.Using != ent.Owner)
            return;
        if (!args.CanAccess || !args.CanInteract)
            return;
        if (!Solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out _, out var stainSolution) || stainSolution.Volume <= 0)
            return;

        var user = args.User;
        var verb = new Verb
        {
            Text = Loc.GetString("stain-verb-wring"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/bubbles.svg.192dpi.png")),
            Act = () =>
            {
                var doAfterArgs = new DoAfterArgs(EntityManager, user, ent.Comp.CleanseDelay, new WringStainDoAfterEvent(), ent.Owner, target: ent.Owner)
                {
                    BreakOnMove = true,
                    BreakOnDamage = true,
                    NeedHand = true,
                    DuplicateCondition = DuplicateConditions.SameTool | DuplicateConditions.SameTarget
                };
                _doAfter.TryStartDoAfter(doAfterArgs);
            },
        };
        args.Verbs.Add(verb);
    }

    private void OnWringDoAfter(Entity<StainableComponent> ent, ref WringStainDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        if (!Solution.TryGetSolution(ent.Owner, ent.Comp.SolutionId, out var stainSoln, out var stainSolution))
            return;

        if (stainSolution.Volume <= 0)
            return;

        var puddleSolution = Solution.SplitSolution(stainSoln.Value, stainSolution.Volume);

        UpdateVisuals(ent);

        if (_puddle.TrySpillAt(args.User, puddleSolution, out _))
            _popup.PopupEntity(Loc.GetString("stain-verb-wring-success", ("item", ent.Owner)), args.User, args.User);
    }
}
