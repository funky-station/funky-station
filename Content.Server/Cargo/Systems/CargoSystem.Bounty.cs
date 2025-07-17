// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Killerqu00 <47712032+Killerqu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 blueDev2 <89804215+blueDev2@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Gansu <68031780+GansuLalan@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 aa5g21 <aa5g21@soton.ac.uk>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Cargo.Components;
using Content.Server.Labels;
using Content.Server.NameIdentifier;
using Content.Server.Research.Systems;
using Content.Shared._Funkystation.Cargo.Prototypes;
using Content.Shared.Access.Components;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.NameIdentifier;
using Content.Shared.Paper;
using Content.Shared.Research.Components;
using Content.Shared.Research.Systems;
using Content.Shared.Stacks;
using Content.Shared.Whitelist;
using JetBrains.Annotations;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

// This file features edits for Funky Station, there are a lot of edits throughout the file

namespace Content.Server.Cargo.Systems;

public sealed partial class CargoSystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly NameIdentifierSystem _nameIdentifier = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSys = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedResearchSystem _research = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

    [ValidatePrototypeId<NameIdentifierGroupPrototype>]
    private const string BountyNameIdentifierGroup = "Bounty";

    private EntityQuery<StackComponent> _stackQuery;
    private EntityQuery<ContainerManagerComponent> _containerQuery;
    private EntityQuery<CargoBountyLabelComponent> _bountyLabelQuery;

    private void InitializeBounty()
    {
        SubscribeLocalEvent<CargoBountyConsoleComponent, BoundUIOpenedEvent>(OnBountyConsoleOpened);
        SubscribeLocalEvent<CargoBountyConsoleComponent, BountyPrintLabelMessage>(OnPrintLabelMessage);
        SubscribeLocalEvent<CargoBountyConsoleComponent, BountySkipMessage>(OnSkipBountyMessage);
        SubscribeLocalEvent<CargoBountyLabelComponent, PriceCalculationEvent>(OnGetBountyPrice);
        SubscribeLocalEvent<EntitySoldEvent>(OnSold);
        SubscribeLocalEvent<StationCargoBountyDatabaseComponent, MapInitEvent>(OnMapInit);

        _stackQuery = GetEntityQuery<StackComponent>();
        _containerQuery = GetEntityQuery<ContainerManagerComponent>();
        _bountyLabelQuery = GetEntityQuery<CargoBountyLabelComponent>();
    }

    private void OnBountyConsoleOpened(EntityUid uid, CargoBountyConsoleComponent component, BoundUIOpenedEvent args)
    {
        if (_station.GetOwningStation(uid) is not { } station ||
            !TryComp<StationCargoBountyDatabaseComponent>(station, out var bountyDb))
            return;

        var untilNextSkip = bountyDb.NextSkipTime - _timing.CurTime;
        _uiSystem.SetUiState(uid,
            CargoConsoleUiKey.Bounty,
            new CargoBountyConsoleState(bountyDb.Bounties, bountyDb.History, untilNextSkip));
    }

    private void OnPrintLabelMessage(EntityUid uid, CargoBountyConsoleComponent component, BountyPrintLabelMessage args)
    {
        if (_timing.CurTime < component.NextPrintTime)
            return;

        if (_station.GetOwningStation(uid) is not { } station)
            return;

        if (!TryGetBountyFromId(station, args.BountyId, out var bounty))
            return;

        var label = Spawn(component.BountyLabelId, Transform(uid).Coordinates);
        component.NextPrintTime = _timing.CurTime + component.PrintDelay;
        SetupBountyLabel(label, station, bounty);
        _audio.PlayPvs(component.PrintSound, uid);
    }

    private void OnSkipBountyMessage(EntityUid uid, CargoBountyConsoleComponent component, BountySkipMessage args)
    {
        if (_station.GetOwningStation(uid) is not { } station ||
            !TryComp<StationCargoBountyDatabaseComponent>(station, out var db))
            return;

        if (_timing.CurTime < db.NextSkipTime)
            return;

        if (!TryGetBountyFromId(station, args.BountyId, out var bounty))
            return;

        if (args.Actor is not { Valid: true } mob)
            return;

        if (TryComp<AccessReaderComponent>(uid, out var accessReaderComponent) &&
            !_accessReaderSystem.IsAllowed(mob, uid, accessReaderComponent))
        {
            _audio.PlayPvs(component.DenySound, uid);
            return;
        }

        if (!TryRemoveBounty(station, bounty, true, args.Actor))
            return;

        FillBountyDatabase(station);
        db.NextSkipTime = _timing.CurTime + db.SkipDelay;
        var untilNextSkip = db.NextSkipTime - _timing.CurTime;
        _uiSystem.SetUiState(uid,
            CargoConsoleUiKey.Bounty,
            new CargoBountyConsoleState(db.Bounties, db.History, untilNextSkip));
        _audio.PlayPvs(component.SkipSound, uid);
    }

    public void SetupBountyLabel(EntityUid uid,
        EntityUid stationId,
        CargoBountyData bounty,
        PaperComponent? paper = null,
        CargoBountyLabelComponent? label = null)
    {
        if (!Resolve(uid, ref paper, ref label))
            return;

        label.Id = bounty.Id;
        label.AssociatedStationId = stationId;
        var msg = new FormattedMessage();
        msg.AddText(Loc.GetString("bounty-manifest-header", ("id", bounty.Id)));
        msg.PushNewline();
        msg.AddText(Loc.GetString("bounty-manifest-list-start"));
        msg.PushNewline();
        foreach (var entry in bounty.Entries)
        {
            switch (entry)
            {
                case CargoObjectBountyItemData objectBounty:
                    msg.AddMarkupOrThrow($"- {Loc.GetString("bounty-console-manifest-entry",
                        ("amount", entry.Amount),
                        ("item", Loc.GetString(entry.Name)))}");
                    msg.PushNewline();
                    break;
                case CargoReagentBountyItemData reagentBounty:
                    msg.AddMarkupOrThrow($"- {Loc.GetString("bounty-console-manifest-entry-reagent",
                        ("amount", entry.Amount),
                        ("item", Loc.GetString(entry.Name)))}");
                    msg.PushNewline();
                    break;
            }
        }
        msg.AddMarkupOrThrow(Loc.GetString("bounty-console-manifest-reward", ("reward", bounty.Reward)));
        _paperSystem.SetContent((uid, paper), msg.ToMarkup());
    }

    /// <summary>
    /// Bounties do not sell for any currency. The reward for a bounty is
    /// calculated after it is sold separately from the selling system.
    /// </summary>
    private void OnGetBountyPrice(EntityUid uid, CargoBountyLabelComponent component, ref PriceCalculationEvent args)
    {
        if (args.Handled || component.Calculating)
            return;

        // make sure this label was actually applied to a crate.
        if (!_container.TryGetContainingContainer((uid, null, null), out var container) ||
            container.ID != LabelSystem.ContainerName)
            return;

        if (component.AssociatedStationId is not { } station ||
            !TryComp<StationCargoBountyDatabaseComponent>(station, out var database))
            return;

        if (database.CheckedBounties.Contains(component.Id))
            return;

        if (!TryGetBountyFromId(station, component.Id, out var bounty, database))
            return;

        if (!IsBountyComplete(container.Owner, bounty))
            return;

        database.CheckedBounties.Add(component.Id);
        args.Handled = true;

        component.Calculating = true;
        args.Price = bounty.Reward - _pricing.GetPrice(container.Owner);
        component.Calculating = false;
    }

    private void OnSold(ref EntitySoldEvent args)
    {
        foreach (var sold in args.Sold)
        {
            if (!TryGetBountyLabel(sold, out _, out var component))
                continue;

            if (component.AssociatedStationId is not { } station ||
                !TryGetBountyFromId(station, component.Id, out var bounty))
            {
                continue;
            }

            if (!IsBountyComplete(sold, bounty))
            {
                continue;
            }

            TryRemoveBounty(station, bounty, false);
            FillBountyDatabase(station);
            _adminLogger.Add(LogType.Action, LogImpact.Low, $"Bounty (id:{bounty.Id}) was fulfilled");
        }
    }

    private bool TryGetBountyLabel(EntityUid uid,
        [NotNullWhen(true)] out EntityUid? labelEnt,
        [NotNullWhen(true)] out CargoBountyLabelComponent? labelComp)
    {
        labelEnt = null;
        labelComp = null;
        if (!_containerQuery.TryGetComponent(uid, out var containerMan))
            return false;

        // make sure this label was actually applied to a crate.
        if (!_container.TryGetContainer(uid, LabelSystem.ContainerName, out var container, containerMan))
            return false;

        if (container.ContainedEntities.FirstOrNull() is not { } label ||
            !_bountyLabelQuery.TryGetComponent(label, out var component))
            return false;

        labelEnt = label;
        labelComp = component;
        return true;
    }

    private void OnMapInit(EntityUid uid, StationCargoBountyDatabaseComponent component, MapInitEvent args)
    {
        FillBountyDatabase(uid, component);
    }

    /// <summary>
    /// Fills up the bounty database with random bounties.
    /// </summary>
    public void FillBountyDatabase(EntityUid uid, StationCargoBountyDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        while (component.Bounties.Count < component.MaxBounties)
        {
            if (!TryAddBounty(uid, component))
                break;
        }

        UpdateBountyConsoles();
    }

    public void RerollBountyDatabase(Entity<StationCargoBountyDatabaseComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        entity.Comp.Bounties.Clear();
        FillBountyDatabase(entity);
    }

    public bool IsBountyComplete(EntityUid container, out HashSet<EntityUid> bountyEntities)
    {
        if (!TryGetBountyLabel(container, out _, out var component))
        {
            bountyEntities = new();
            return false;
        }

        var station = component.AssociatedStationId;
        if (station == null)
        {
            bountyEntities = new();
            return false;
        }

        if (!TryGetBountyFromId(station.Value, component.Id, out var bounty))
        {
            bountyEntities = new();
            return false;
        }

        return IsBountyComplete(container, bounty, out bountyEntities);
    }

    public bool IsBountyComplete(EntityUid container, CargoBountyData data)
    {
        return IsBountyComplete(container, data, out _);
    }

    public bool IsBountyComplete(EntityUid container, CargoBountyData data, out HashSet<EntityUid> bountyEntities)
    {

        return IsBountyComplete(container, data.Entries, out bountyEntities);
    }

    public bool IsBountyComplete(EntityUid container, string id)
    {
        if (!_protoMan.TryIndex<CargoBountyPrototype>(id, out var proto))
            return false;

        var items = new HashSet<CargoBountyItemData>();
        foreach (var entry in proto.Entries)
        {
            CargoBountyItemData newItem = entry switch
            {
                CargoObjectBountyItemEntry itemEntry => new CargoObjectBountyItemData(itemEntry),
                CargoReagentBountyItemEntry itemEntry => new CargoReagentBountyItemData(itemEntry),
                _ => throw new NotImplementedException($"Unknown type: {entry.GetType().Name}"),
            };
            items.Add(newItem);

        }

        return IsBountyComplete(container, items);
    }

    public bool IsBountyComplete(EntityUid container, CargoBountyPrototype prototype)
    {
        var items = new HashSet<CargoBountyItemData>();
        foreach (var entry in prototype.Entries)
        {
            CargoBountyItemData newItem = entry switch
            {
                CargoObjectBountyItemEntry itemEntry => new CargoObjectBountyItemData(itemEntry),
                CargoReagentBountyItemEntry itemEntry => new CargoReagentBountyItemData(itemEntry),
                _ => throw new NotImplementedException($"Unknown type: {entry.GetType().Name}"),
            };
            items.Add(newItem);
        }

        return IsBountyComplete(container, items);
    }

    public bool IsBountyComplete(EntityUid container, IEnumerable<CargoBountyItemData> entries)
    {
        return IsBountyComplete(container, entries, out _);
    }

    public bool IsBountyComplete(EntityUid container,
        IEnumerable<CargoBountyItemData> entries,
        out HashSet<EntityUid> bountyEntities)
    {
        return IsBountyComplete(GetBountyEntities(container), entries, out bountyEntities);
    }

    /// <summary>
    /// Determines whether the <paramref name="entity"/> meets the criteria for the bounty <paramref name="entry"/>.
    /// </summary>
    /// <returns>true if <paramref name="entity"/> is a valid item for the bounty entry, otherwise false</returns>
    public bool IsValidBountyEntry(EntityUid entity, CargoObjectBountyItemData entry)
    {
        if (!_whitelistSys.IsValid(entry.Whitelist, entity))
            return false;

        if (entry.Blacklist != null && _whitelistSys.IsValid(entry.Blacklist, entity))
            return false;

        return true;
    }

    /// <summary>
    /// Determines whether the <paramref name="entity"/> meets the criteria for the bounty <paramref name="entry"/>.
    /// </summary>
    /// <param name="entity">Some given entity to be checked against criteria</param>
    /// <param name="reagentBounty">The specific bounty reagent item that is being checked against</param>
    /// <returns>true if <paramref name="entity"/> is a valid item for the bounty entry, otherwise false</returns>
    public bool IsValidBountyEntry(EntityUid entity, CargoReagentBountyItemData reagentBounty)
    {
        if (!TryComp<SolutionContainerManagerComponent>(entity, out var solutions))
            return false;

        if (!_protoMan.TryIndex(reagentBounty.Reagent, out var bounty))
            return false;

        foreach (var (_, soln) in _solutionContainer.EnumerateSolutions((entity, solutions)))
        {
            var solution = soln.Comp.Solution;

            foreach (var sol in solution.Contents)
            {
                if (sol.Reagent.Prototype.Equals(bounty.ID))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the <paramref name="entity"/> meets the criteria for the bounty <paramref name="entry"/>.
    /// </summary>
    /// <returns>true if <paramref name="entity"/> is a valid item for the bounty entry, otherwise false</returns>
    public bool IsValidBountyEntry(EntityUid entity, CargoBountyItemEntry entry)
    {
        return entry switch
        {
            CargoObjectBountyItemEntry objectBounty =>
                IsValidBountyEntry(entity, new CargoObjectBountyItemData(objectBounty)),
            CargoReagentBountyItemEntry reagentBounty =>
                IsValidBountyEntry(entity, new CargoReagentBountyItemData(reagentBounty)),
            _ => throw new NotImplementedException($"Unknown type: {entry.GetType().Name}"),
        };
    }

    /// <summary>
    /// Checks if some bounty is complete by iterating through a given set of entities and then matching
    /// them to potential bounty objectives
    /// </summary>
    /// <param name="entities">Given set of entities to match</param>
    /// <param name="entries">Given list of bounties objectives to match to</param>
    /// <param name="bountyEntities">Returns a list of entites that are used to fufil the bounty, is a subset of <paramref name="entities"/></param>
    /// <returns>True if the given bounty objectives are passed, false otherwise</returns>
    public bool IsBountyComplete(HashSet<EntityUid> entities,
        IEnumerable<CargoBountyItemData> entries,
        out HashSet<EntityUid> bountyEntities)
    {
        bountyEntities = new();

        var entityReqs = new Dictionary<EntityUid, HashSet<CargoBountyItemData>>();

        // Matches the given entities to potential objectives each item can fufill
        foreach (var entity in entities)
        {
            entityReqs.Add(entity, new HashSet<CargoBountyItemData>());
            foreach (var entry in entries)
            {
                switch (entry)
                {
                    case CargoObjectBountyItemData objectBounty:
                        if (!IsValidBountyEntry(entity, objectBounty))
                            continue;
                        break;
                    case CargoReagentBountyItemData reagentBounty:
                        if (!IsValidBountyEntry(entity, reagentBounty))
                            continue;
                        break;
                }
                entityReqs[entity].Add(entry);
            }
        }

        var remaining = new Dictionary<CargoBountyItemData, int>();
        foreach (var e in entries)
        {
            remaining[e] = e.Amount;
        }

        // Matches entities to bounty objectives, for object bounties each item can only be matched once, but as
        // solutions can hold multiple different solutions we must consider how to match multiple objectives per
        // entity for reagents
        var sorted = entityReqs.OrderBy(kvp => kvp.Value.Count).ToList();
        foreach (var (entity, possibleEntries) in sorted)
        {
            var chosenEntry = possibleEntries.FirstOrDefault(b => remaining.ContainsKey(b) && remaining[b] > 0);

            if (chosenEntry == null)
                continue;
            bountyEntities.Add(entity);
            switch (chosenEntry)
            {
                case CargoObjectBountyItemData bountyItem:
                    remaining[chosenEntry]--;
                    break;
                case CargoReagentBountyItemData bountyItem:
                    // TODO: This is horrible and I hate it, but I am bad and need to study to implement it better
                    if (!TryComp<SolutionContainerManagerComponent>(entity, out var solutions))
                        continue;
                    foreach (var (_, soln) in _solutionContainer.EnumerateSolutions((entity, solutions)))
                    {
                        var solution = soln.Comp.Solution;

                        foreach (var sol in solution.Contents)
                        {
                            foreach (var cargoBountyItemData1 in possibleEntries)
                            {
                                var cargoBountyItemData = (CargoReagentBountyItemData)cargoBountyItemData1;
                                if (sol.Reagent.Prototype.Equals(cargoBountyItemData.Reagent.Id))
                                    remaining[cargoBountyItemData] -= sol.Quantity.Value / 100;
                            }

                        }
                    }
                    break;
            }
        }

        foreach (var e in remaining)
        {
            if (e.Value > 0)
            {
                return false;
            }
        }

        return true;
    }

    private HashSet<EntityUid> GetBountyEntities(EntityUid uid)
    {
        var entities = new HashSet<EntityUid>
        {
            uid
        };
        if (!TryComp<ContainerManagerComponent>(uid, out var containers))
            return entities;

        foreach (var container in containers.Containers.Values)
        {
            foreach (var ent in container.ContainedEntities)
            {
                if (_bountyLabelQuery.HasComponent(ent))
                    continue;

                var children = GetBountyEntities(ent);
                foreach (var child in children)
                {
                    entities.Add(child);
                }
            }
        }

        return entities;
    }
    // Beginning of major Funky Station Edits
    /// <summary>
    /// This method will attempt to add a bounty to a given station bounty database
    /// </summary>
    /// <param name="uid">The uid of the entity trying to add the item, this is normally the bounty computer</param>
    /// <param name="component">The bounty database for a station, each station has one though we normally don't have
    /// any outside the main station</param>
    /// <returns>True if the bounty is successfully added, false otherwise</returns>
    /// <exception cref="NotImplementedException">This will be thrown if some bounty type that handling has not be
    /// created for is attempted to be made</exception>
    [PublicAPI]
    public bool TryAddBounty(EntityUid uid, StationCargoBountyDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (component.Bounties.Count >= component.MaxBounties)
            return false;

        var allBounties = _protoMan.EnumeratePrototypes<CargoBountyCategoryPrototype>().ToList();
        if (allBounties.Count < 1)
            return false;
        var chosenCategory = false;
        var bountyCategory = _random.Pick(allBounties);
        List<CargoBountyItemEntry> bountyItems = [];
        while (!chosenCategory)
        {
            // This all feels wrong, but it works so hey ho
            var duplicheck = true;
            while (duplicheck)
            {
                duplicheck = false;
                bountyCategory = _random.Pick(allBounties);
                foreach (var entry in component.Bounties)
                {
                    if (entry.Category == bountyCategory.Name)
                    {
                        duplicheck = true;
                    }
                }
            }

            chosenCategory = CheckCategory(uid, bountyCategory, out var availableBounties);
            if (chosenCategory)
            {
                bountyItems = availableBounties;
                continue;
            }

            allBounties.Remove(bountyCategory);

            if (allBounties.Count == 0)
            {
                Log.Error("Failed to add bounty because there are no categories available");
                return false;
            }

            bountyCategory = _random.Pick(allBounties);
        }

        var totalItems = bountyItems.Count;

        // Smaller number means that there will be on average less item per bounty
        const double itemNumberWeight = 0.9;
        var selection = Math.Min(1 - Math.Ceiling(Math.Log(Math.Pow(_random.NextDouble(), itemNumberWeight), 2)),
            totalItems);
        var totalReward = 0;
        var newBounty = new CargoBountyData
        {
            IdPrefix = bountyCategory.IdPrefix,
            Category = bountyCategory.Name,
        };
        var totalBountyItems = 0;

        for (var i = 1; i <= selection;)
        {
            if (!SelectBountyEntry(bountyItems, out var bountyItem))
            {
                return false;
            }

            var skip = false;
            foreach (var entry in newBounty.Entries)
            {
                if (entry.Name == bountyItem.Name)
                {
                    skip = true;
                }
            }
            if (skip)
                continue;

            CargoBountyItemData bountyItemData = bountyItem switch
            {
                CargoObjectBountyItemEntry itemEntry => new CargoObjectBountyItemData(itemEntry),
                CargoReagentBountyItemEntry itemEntry => new CargoReagentBountyItemData(itemEntry),
                _ => throw new NotImplementedException($"Unknown type: {bountyItem.GetType().Name}"),
            };

            var steps = (bountyItem.MaxAmount - bountyItem.MinAmount) / bountyItem.AmountStep;
            var step = _random.Next(steps + 1);
            var bountyAmount = step * bountyItem.AmountStep + bountyItem.MinAmount;
            totalReward += bountyAmount * bountyItem.RewardPer;
            bountyItemData.Amount = bountyAmount;

            // Counter for the total number of bounty items, used for if the number goes over 30 (basic crate limit)
            switch (bountyItemData)
            {
                case CargoObjectBountyItemData objectBounty:
                    totalBountyItems += bountyAmount;
                    break;
                case CargoReagentBountyItemData reagentBounty:
                    totalBountyItems ++;
                    break;
            }

            newBounty.Entries.Add(bountyItemData);
            if (totalItems > 1)
                totalItems--;

            i++;
        }

        newBounty.Reward = totalReward;
        _nameIdentifier.GenerateUniqueName(uid, BountyNameIdentifierGroup, out var randomVal);
        newBounty.Id = $"{newBounty.IdPrefix}{randomVal:D3}";
        newBounty.Description = Loc.GetString("bounty-console-category-description",
            ("category", Loc.GetString(bountyCategory.Name)),
            ("id", newBounty.Id));

        if (totalBountyItems > 30)
        {
            newBounty.Description += " (This bounty requires more compact storage methods such as cardboard boxes or bags)";
        }
        if (component.Bounties.Any(b => b.Id == newBounty.Id))
        {
            Log.Error("Failed to add bounty {ID} because another one with the same ID already existed!", newBounty.Id);
            return false;
        }

        component.Bounties.Add(newBounty);
        component.TotalBounties++;
        return true;
    }

    /// <summary>
    /// Selects a bounty item from a list of entries accounting for the entries weightings.
    /// </summary>
    /// <param name="entries">List of entries to select from.</param>
    /// <param name="bountyEntry">The randomly selected entry.</param>
    /// <returns>True of false depending on the success of the selection.</returns>
    private bool SelectBountyEntry(List<CargoBountyItemEntry> entries, out CargoBountyItemEntry bountyEntry)
    {
        double totalWeight = 0;
        foreach (var entry in entries)
        {
            totalWeight += entry.Weight;
        }
        var roll = _random.NextDouble(0, totalWeight);

        foreach (var entry in entries)
        {
            roll -= entry.Weight;
            if (!(roll <= 0))
                continue;
            bountyEntry = entry;
            return true;
        }

        bountyEntry = new CargoObjectBountyItemEntry();
        return false;
    }

    /// <summary>
    /// Checks if a given bounty category is valid to be created for and returns a list of valid objectives from the
    /// category that can have bounties created from
    /// </summary>
    /// <param name="uid">The entity try to create a bounty</param>
    /// <param name="category">Some given bounty category as defined in yml</param>
    /// <param name="availableBounties">Returns a list of currently valid objectives</param>
    /// <returns>True if the category can have bounties created for, false otherwise</returns>
    private bool CheckCategory(EntityUid uid, CargoBountyCategoryPrototype category, out List<CargoBountyItemEntry> availableBounties)
    {
        var bountyItems = new List<CargoBountyItemEntry>(category.Entries);
        List<CargoBountyItemEntry> toRemove = new();
        foreach (var bountyEntry in bountyItems)
        {
            switch (bountyEntry)
            {
                case CargoObjectBountyItemEntry bountyItem:
                    if (bountyItem.RequiredResearch == null)
                        continue;

                    List<bool> techChecks = [];
                    foreach (var research in bountyItem.RequiredResearch)
                    {

                        var query = EntityManager.EntityQueryEnumerator<TechnologyDatabaseComponent>();

                        while (query.MoveNext(out var tEntityUid, out var technologyDatabaseComponent))
                        {
                            if (_station.GetOwningStation(uid) is { } station &&
                                _station.GetOwningStation(tEntityUid) != station)
                                continue;
                            techChecks.Add(
                                _research.IsTechnologyUnlocked(tEntityUid, (string) research, technologyDatabaseComponent));
                            break;
                        }
                    }

                    if (techChecks.Count == 0 || !techChecks.Any(techCheck => techCheck))
                    {
                        toRemove.Add(bountyItem);
                    }
                    break;
                case CargoReagentBountyItemEntry bountyItem:
                    continue;
            }
        }

        bountyItems.RemoveAll(b => toRemove.Contains(b));

        if (bountyItems.Count == 0)
        {
            availableBounties = [];
            return false;
        }

        availableBounties = bountyItems;
        return true;
    }

    [PublicAPI]
    public bool TryAddBounty(EntityUid uid, string bountyId, StationCargoBountyDatabaseComponent? component = null)
    {
        if (!_protoMan.TryIndex<CargoBountyPrototype>(bountyId, out var bounty))
        {
            return false;
        }

        if (!Resolve(uid, ref component))
            return false;

        if (component.Bounties.Count >= component.MaxBounties)
            return false;

        _nameIdentifier.GenerateUniqueName(uid, BountyNameIdentifierGroup, out var randomVal);
        var newBounty = new CargoBountyData(randomVal, bounty);
        // This bounty id already exists! Probably because NameIdentifierSystem ran out of ids.
        if (component.Bounties.Any(b => b.Id == newBounty.Id))
        {
            Log.Error("Failed to add bounty {ID} because another one with the same ID already existed!", newBounty.Id);
            return false;
        }
        component.Bounties.Add(new CargoBountyData(randomVal, bounty));
        _adminLogger.Add(LogType.Action,
            LogImpact.Low,
            $"Added bounty \"{bounty.ID}\" (id:{component.TotalBounties}) to station {ToPrettyString(uid)}");
        component.TotalBounties++;
        return true;
    }
// End of major Funky Station Edits

    [PublicAPI]
    public bool TryRemoveBounty(Entity<StationCargoBountyDatabaseComponent?> ent,
        string dataId,
        bool skipped,
        EntityUid? actor = null)
    {
        if (!TryGetBountyFromId(ent.Owner, dataId, out var data, ent.Comp))
            return false;

        return TryRemoveBounty(ent, data, skipped, actor);
    }

    public bool TryRemoveBounty(Entity<StationCargoBountyDatabaseComponent?> ent,
        CargoBountyData data,
        bool skipped,
        EntityUid? actor = null)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        for (var i = 0; i < ent.Comp.Bounties.Count; i++)
        {
            if (ent.Comp.Bounties[i].Id == data.Id)
            {
                string? actorName = null;
                if (actor != null)
                {
                    var getIdentityEvent = new TryGetIdentityShortInfoEvent(ent.Owner, actor.Value);
                    RaiseLocalEvent(getIdentityEvent);
                    actorName = getIdentityEvent.Title;
                }

                ent.Comp.History.Add(new CargoBountyHistoryData(data,
                    skipped
                        ? CargoBountyHistoryData.BountyResult.Skipped
                        : CargoBountyHistoryData.BountyResult.Completed,
                    _timing.CurTime,
                    actorName));
                ent.Comp.Bounties.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public bool TryGetBountyFromId(
        EntityUid uid,
        string id,
        [NotNullWhen(true)] out CargoBountyData? bounty,
        StationCargoBountyDatabaseComponent? component = null)
    {
        bounty = null;
        if (!Resolve(uid, ref component))
            return false;

        foreach (var bountyData in component.Bounties)
        {
            if (bountyData.Id != id)
                continue;
            bounty = bountyData;
            break;
        }

        return bounty != null;
    }

    public void UpdateBountyConsoles()
    {
        var query = EntityQueryEnumerator<CargoBountyConsoleComponent, UserInterfaceComponent>();
        while (query.MoveNext(out var uid, out _, out var ui))
        {
            if (_station.GetOwningStation(uid) is not { } station ||
                !TryComp<StationCargoBountyDatabaseComponent>(station, out var db))
            {
                continue;
            }

            var untilNextSkip = db.NextSkipTime - _timing.CurTime;
            _uiSystem.SetUiState((uid, ui), CargoConsoleUiKey.Bounty, new CargoBountyConsoleState(db.Bounties, db.History, untilNextSkip));
        }
    }

    private void UpdateBounty()
    {
        var query = EntityQueryEnumerator<StationCargoBountyDatabaseComponent>();
        while (query.MoveNext(out var bountyDatabase))
        {
            bountyDatabase.CheckedBounties.Clear();
        }
    }
}
