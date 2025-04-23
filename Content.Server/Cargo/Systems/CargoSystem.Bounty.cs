using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Cargo.Components;
using Content.Server.Labels;
using Content.Server.NameIdentifier;
using Content.Shared._Funkystation.Cargo.Prototypes;
using Content.Shared.Access.Components;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.NameIdentifier;
using Content.Shared.Paper;
using Content.Shared.Stacks;
using Content.Shared.Whitelist;
using JetBrains.Annotations;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

// This file features edits for Funky Station, while there are minor edits throughout the file, the section of major changes has been marked

namespace Content.Server.Cargo.Systems;

public sealed partial class CargoSystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly NameIdentifierSystem _nameIdentifier = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSys = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

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
        _uiSystem.SetUiState(uid, CargoConsoleUiKey.Bounty, new CargoBountyConsoleState(bountyDb.Bounties, bountyDb.History, untilNextSkip));
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
        if (_station.GetOwningStation(uid) is not { } station || !TryComp<StationCargoBountyDatabaseComponent>(station, out var db))
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
        _uiSystem.SetUiState(uid, CargoConsoleUiKey.Bounty, new CargoBountyConsoleState(db.Bounties, db.History, untilNextSkip));
        _audio.PlayPvs(component.SkipSound, uid);
    }

    public void SetupBountyLabel(EntityUid uid, EntityUid stationId, CargoBountyData bounty, PaperComponent? paper = null, CargoBountyLabelComponent? label = null)
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
            msg.AddMarkupOrThrow($"- {Loc.GetString("bounty-console-manifest-entry",
                ("amount", entry.Amount),
                ("item", Loc.GetString(entry.Name)))}");
            msg.PushNewline();
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
        if (!_container.TryGetContainingContainer((uid, null, null), out var container) || container.ID != LabelSystem.ContainerName)
            return;

        if (component.AssociatedStationId is not { } station || !TryComp<StationCargoBountyDatabaseComponent>(station, out var database))
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

            if (component.AssociatedStationId is not { } station || !TryGetBountyFromId(station, component.Id, out var bounty))
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
            items.Add(new CargoBountyItemData(entry));
        }

        return IsBountyComplete(container, items);
    }

    public bool IsBountyComplete(EntityUid container, CargoBountyPrototype prototype)
    {
        var items = new HashSet<CargoBountyItemData>();
        foreach (var entry in prototype.Entries)
        {
            items.Add(new CargoBountyItemData(entry));
        }

        return IsBountyComplete(container, items);
    }

    public bool IsBountyComplete(EntityUid container, IEnumerable<CargoBountyItemData> entries)
    {
        return IsBountyComplete(container, entries, out _);
    }

    public bool IsBountyComplete(EntityUid container, IEnumerable<CargoBountyItemData> entries, out HashSet<EntityUid> bountyEntities)
    {
        return IsBountyComplete(GetBountyEntities(container), entries, out bountyEntities);
    }

    /// <summary>
    /// Determines whether the <paramref name="entity"/> meets the criteria for the bounty <paramref name="entry"/>.
    /// </summary>
    /// <returns>true if <paramref name="entity"/> is a valid item for the bounty entry, otherwise false</returns>
    public bool IsValidBountyEntry(EntityUid entity, CargoBountyItemData entry)
    {
        if (!_whitelistSys.IsValid(entry.Whitelist, entity))
            return false;

        if (entry.Blacklist != null && _whitelistSys.IsValid(entry.Blacklist, entity))
            return false;

        return true;
    }

    public bool IsValidBountyEntry(EntityUid entity, CargoBountyItemEntry entry)
    {
        return IsValidBountyEntry(entity, new CargoBountyItemData(entry));
    }

    public bool IsBountyComplete(HashSet<EntityUid> entities, IEnumerable<CargoBountyItemData> entries, out HashSet<EntityUid> bountyEntities)
    {
        bountyEntities = new();

        var entityReqs = new Dictionary<EntityUid, HashSet<CargoBountyItemData>>();

        foreach (var entity in entities)
        {
            entityReqs.Add(entity, new HashSet<CargoBountyItemData>());
            foreach (var entry in entries)
            {
                if (!IsValidBountyEntry(entity, entry))
                    continue;

                entityReqs[entity].Add(entry);
            }
        }

        var remaining = new Dictionary<CargoBountyItemData, int>();
        foreach (var e in entries)
        {
            remaining[e] = e.Amount;
        }

        var sorted = entityReqs.OrderBy(kvp => kvp.Value.Count).ToList();
        foreach (var (entity, possibleEntries) in sorted)
        {
            var chosenEntry = possibleEntries.FirstOrDefault(b => remaining.ContainsKey(b) && remaining[b] > 0);

            if (chosenEntry == null)
                continue;
            bountyEntities.Add(entity);
            remaining[chosenEntry]--;
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
    [PublicAPI]
    public bool TryAddBounty(EntityUid uid, StationCargoBountyDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (component.Bounties.Count >= component.MaxBounties)
            return false;

        var allBounties = _protoMan.EnumeratePrototypes<CargoBountyCategoryPrototype>().ToList();
        var bountyCategory = _random.Pick(allBounties);
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

        var totalItems = bountyCategory.Entries.Count;

        // Smaller number means that there will be on average less item per bounty
        const double itemNumberWeight = 0.9;
        var selection = Math.Min(1 - Math.Ceiling(Math.Log(Math.Pow(_random.NextDouble(), itemNumberWeight), 2)), totalItems);
        var totalReward = 0;
        var newBounty = new CargoBountyData();
        newBounty.IdPrefix = bountyCategory.IdPrefix;
        newBounty.Category = bountyCategory.Name;
        var totalBountyItems = 0;
        for (var i = 1; i <= selection;)
        {
            var bountyItem = _random.Pick(bountyCategory.Entries);

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

            var bountyItemData = new CargoBountyItemData(bountyItem);
            var bountyAmount = _random.Next(bountyItem.MinAmount, bountyItem.MaxAmount);
            totalReward += bountyAmount * bountyItem.RewardPer;
            bountyItemData.Amount = bountyAmount;
            totalBountyItems += bountyAmount;
            newBounty.Entries.Add(bountyItemData);
            if (totalItems > 1)
                totalItems--;

            i++;
        }

        newBounty.Reward = totalReward;
        _nameIdentifier.GenerateUniqueName(uid, BountyNameIdentifierGroup, out var randomVal);
        newBounty.Id = $"{newBounty.IdPrefix}{randomVal:D3}";
        newBounty.Description = Loc.GetString("bounty-console-category-description", ("category", Loc.GetString(bountyCategory.Name)), ("id", newBounty.Id));

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
        _adminLogger.Add(LogType.Action, LogImpact.Low, $"Added bounty \"{bounty.ID}\" (id:{component.TotalBounties}) to station {ToPrettyString(uid)}");
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
                    _gameTiming.CurTime,
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
