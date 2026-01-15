using System.Linq;
using Content.Shared._Funkystation.Cargo.Components;
using Content.Shared.Atmos.Components;
using Content.Shared.Cargo.Components;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Server.Cargo.Systems;
using Robust.Shared.Audio.Systems;
using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Server.Popups;
using Content.Shared.Access.Systems;
using Content.Server.Station.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Audio;
using Content.Shared.Cargo;
using Content.Server.Radio.EntitySystems;
using Content.Shared.IdentityManagement;
using Robust.Shared.Prototypes;
using Content.Shared.Atmos.Prototypes;

namespace Content.Server._Funkystation.Cargo.Systems;

public sealed partial class GasMinerConsoleSystem : SharedCargoSystem
{
    [Dependency] private readonly CargoSystem _cargo = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    private static readonly SoundPathSpecifier ApproveSound = new("/Audio/Effects/Cargo/ping.ogg");

    public override void Initialize()
    {
        SubscribeLocalEvent<GasMinerConsoleComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GasMinerConsoleComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<CargoOrderConsoleComponent, GasMinerSetSettingsMessage>(OnSetSettings);
        SubscribeLocalEvent<CargoOrderConsoleComponent, BuyMolesForMinerMessage>(OnBuyMolesForMiner);
        SubscribeLocalEvent<CargoOrderConsoleComponent, ToggleAutoBuyMinerMessage>(OnToggleAutoBuyMiner);
    }

    private void OnToggleAutoBuyMiner(Entity<CargoOrderConsoleComponent> ent, ref ToggleAutoBuyMinerMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        if (!TryComp<GasMinerConsoleComponent>(ent, out var gasConsole))
            return;

        if (args.MinerIndex < 0 || args.MinerIndex >= gasConsole.LinkedMiners.Count)
            return;

        var minerUid = gasConsole.LinkedMiners[args.MinerIndex];
        if (!TryComp<GasMinerComponent>(minerUid, out var miner))
            return;

        // Access check
        if (!_accessReader.IsAllowed(actor, ent.Owner))
        {
            _popup.PopupEntity(Loc.GetString("cargo-console-order-not-allowed"), actor, actor);
            _audio.PlayPredicted(ent.Comp.ErrorSound ?? default, ent, actor);
            return;
        }

        miner.AutoBuyEnabled = args.Enabled;
        Dirty(minerUid, miner);

        var tryGetIdentity = new TryGetIdentityShortInfoEvent(ent, actor);
        RaiseLocalEvent(tryGetIdentity);

        var playerName = tryGetIdentity.Title ?? Loc.GetString("cargo-console-fund-transfer-user-unknown");
        var accountProto = _proto.Index(ent.Comp.Account);

        var key = args.Enabled
            ? "gas-miner-miner-autobuy-enabled"
            : "gas-miner-miner-autobuy-disabled";

        var msg = Loc.GetString(key,
            ("name", playerName),
            ("gas", Loc.GetString(_proto.Index<GasPrototype>(((int)miner.SpawnGas).ToString()).Name)));

        _radio.SendRadioMessage(ent, msg, accountProto.RadioChannel, ent, escapeMarkup: false);

        _adminLog.Add(LogType.Action, LogImpact.Low,
            $"{ToPrettyString(actor):player} {(args.Enabled ? "enabled" : "disabled")} auto-buy on miner {ToPrettyString(minerUid)} (gas: {miner.SpawnGas}) at console {ToPrettyString(ent)}");
    }

    private void OnBuyMolesForMiner(Entity<CargoOrderConsoleComponent> ent, ref BuyMolesForMinerMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        if (!TryComp<GasMinerConsoleComponent>(ent, out var gasConsole))
            return;

        if (args.MinerIndex < 0 || args.MinerIndex >= gasConsole.LinkedMiners.Count)
            return;

        var minerUid = gasConsole.LinkedMiners[args.MinerIndex];
        if (!TryComp<GasMinerComponent>(minerUid, out var miner))
            return;

        // Access check
        if (!_accessReader.IsAllowed(actor, ent.Owner))
        {
            _popup.PopupEntity(Loc.GetString("cargo-console-order-not-allowed"), actor, actor);
            _audio.PlayPredicted(ent.Comp.ErrorSound ?? default, ent, actor);
            return;
        }

        int specoAmount = args.SpecoAmount;
        if (specoAmount <= 0)
            return;

        var station = _station.GetOwningStation(ent);
        if (station == null || !TryComp<StationBankAccountComponent>(station, out var bank))
        {
            return;
        }

        var accountId = ent.Comp.Account;
        var currentBalance = _cargo.GetBalanceFromAccount((station.Value, bank), accountId);
        if (currentBalance < specoAmount)
        {
            _popup.PopupEntity(Loc.GetString("cargo-console-insufficient-funds", ("cost", specoAmount)), actor, actor);
            _audio.PlayPredicted(ent.Comp.ErrorSound ?? default, ent, actor);
            return;
        }

        // Get price per mole for this gas
        string gasId = ((int)miner.SpawnGas).ToString();
        if (!_proto.TryIndex<GasPrototype>(gasId, out var gasProto) || gasProto.PricePerMole <= 0)
        {
            _popup.PopupEntity("Gas has no valid price!", actor, actor); // free gases cannot be purchased, use miners instead
            return;
        }

        float costPerMole = gasProto.PricePerMole * gasConsole.PriceMultiplier;

        // Calculate moles received
        float molesToAdd = specoAmount / costPerMole;

        // Apply transaction
        _cargo.WithdrawFunds((station.Value, bank), -specoAmount, accountId);

        miner.RemainingMoles += molesToAdd;
        Dirty(minerUid, miner);

        _audio.PlayPvs(ApproveSound, ent);
        _popup.PopupEntity(Loc.GetString("gas-miner-moles-purchase-success",
            ("moles", molesToAdd.ToString("F1")),
            ("gas", Loc.GetString(gasProto.Name)),
            ("spesos", specoAmount)), actor, actor);

        _adminLog.Add(LogType.Action, LogImpact.Medium,
            $"{ToPrettyString(actor):player} purchased {molesToAdd:F1} moles of {miner.SpawnGas} for {specoAmount} spesos at console {ToPrettyString(ent)} (miner: {ToPrettyString(minerUid)})");

        var tryGetIdentity = new TryGetIdentityShortInfoEvent(ent, actor);
        RaiseLocalEvent(tryGetIdentity);

        var playerName = tryGetIdentity.Title ?? Loc.GetString("cargo-console-fund-transfer-user-unknown");
        var accountProto = _proto.Index(accountId);

        var msg = Loc.GetString("gas-miner-moles-purchase-broadcast",
            ("name", playerName),
            ("moles", molesToAdd.ToString("F1")),
            ("gas", Loc.GetString(gasProto.Name)),
            ("spesos", specoAmount),
            ("accountName", Loc.GetString(accountProto.Name)),
            ("accountCode", Loc.GetString(accountProto.Code)));

        _radio.SendRadioMessage(ent, msg, accountProto.RadioChannel, ent, escapeMarkup: false);
    }

    public bool TryAutoPurchaseMoles(EntityUid consoleUid, CargoOrderConsoleComponent orderConsole, EntityUid minerUid, float desiredMoles)
    {
        if (!TryComp<GasMinerComponent>(minerUid, out var miner))
            return false;

        if (!TryComp<GasMinerConsoleComponent>(consoleUid, out var gasConsole))
            return false;

        string gasId = ((int)miner.SpawnGas).ToString();
        if (!_proto.TryIndex<GasPrototype>(gasId, out var gasProto) || gasProto.PricePerMole <= 0)
            return false;

        float costPerMole = gasProto.PricePerMole * gasConsole.PriceMultiplier;
        int spesosNeeded = (int)Math.Ceiling(desiredMoles * costPerMole);

        var station = _station.GetOwningStation(consoleUid);
        if (station == null || !TryComp<StationBankAccountComponent>(station, out var bank))
            return false;

        var accountId = orderConsole.Account;
        if (_cargo.GetBalanceFromAccount((station.Value, bank), accountId) < spesosNeeded)
            return false;

        // Withdraw
        _cargo.WithdrawFunds((station.Value, bank), -spesosNeeded, accountId);

        // Add moles directly to this miner
        miner.RemainingMoles += desiredMoles;
        Dirty(minerUid, miner);

        return true;
    }

    private void OnStartup(Entity<GasMinerConsoleComponent> ent, ref ComponentStartup args)
    {
        SyncLinkedMiners(ent);
    }

    private void OnNewLink(Entity<GasMinerConsoleComponent> ent, ref NewLinkEvent args)
    {
        if (args.Source != ent.Owner)
            return;

        if (!TryComp<DeviceLinkSourceComponent>(ent.Owner, out _))
            return;

        if (HasComp<GasMinerComponent>(args.Sink))
            SyncLinkedMiners(ent);
    }

    private void OnSetSettings(Entity<CargoOrderConsoleComponent> ent, ref GasMinerSetSettingsMessage args)
    {
        if (!TryComp<GasMinerConsoleComponent>(ent, out var gasConsole))
            return;

        if (args.MinerIndex < 0 || args.MinerIndex >= gasConsole.LinkedMiners.Count)
            return;

        var minerUid = gasConsole.LinkedMiners[args.MinerIndex];

        if (!TryComp<GasMinerComponent>(minerUid, out var miner))
            return;

        miner.SpawnAmount = args.NewSpawnAmount;
        miner.MaxExternalPressure = args.NewMaxExternalPressure;

        Dirty(minerUid, miner);
    }

    private void SyncLinkedMiners(Entity<GasMinerConsoleComponent> ent)
    {
        if (!TryComp<DeviceLinkSourceComponent>(ent.Owner, out var sourceComp))
            return;

        var newMiners = sourceComp.LinkedPorts.Keys
            .Where(sink => HasComp<GasMinerComponent>(sink))
            .ToList();

        var comp = ent.Comp;

        if (!comp.LinkedMiners.SequenceEqual(newMiners))
        {
            comp.LinkedMiners.Clear();
            comp.LinkedMiners.AddRange(newMiners);
            Dirty(ent, comp);
        }
    }
}
