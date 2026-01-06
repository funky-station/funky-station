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
        SubscribeLocalEvent<GasMinerConsoleComponent, BuyGasCreditsMessage>(OnBuyGasCredits);
        SubscribeLocalEvent<GasMinerConsoleComponent, AutoBuyToggleMessage>(OnAutoBuyToggle);
    }

    private void OnAutoBuyToggle(Entity<GasMinerConsoleComponent> ent, ref AutoBuyToggleMessage args)
    {
        if (args.Actor == null)
            return;

        var actor = args.Actor;

        // Access check
        if (!EntityManager.TryGetComponent<CargoOrderConsoleComponent>(ent, out var orderConsole) || !_accessReader.IsAllowed(actor, ent))
        {
            _popup.PopupEntity(Loc.GetString("cargo-console-order-not-allowed"), actor, actor);
            _audio.PlayPredicted(orderConsole?.ErrorSound ?? default, ent, actor);
            return;
        }

        ent.Comp.AutoBuy = args.Enabled;
        Dirty(ent);

        var tryGetIdentity = new TryGetIdentityShortInfoEvent(ent, actor);
        RaiseLocalEvent(tryGetIdentity);

        var playerName = tryGetIdentity.Title ?? Loc.GetString("cargo-console-fund-transfer-user-unknown");
        var accountProto = _proto.Index(orderConsole.Account);

        var key = args.Enabled
            ? "gas-miner-autobuy-enabled-broadcast"
            : "gas-miner-autobuy-disabled-broadcast";

        var msg = Loc.GetString(key, ("name", playerName));

        _radio.SendRadioMessage(ent, msg, accountProto.RadioChannel, ent, escapeMarkup: false);

        _adminLog.Add(LogType.Action, LogImpact.Low,
            $"{ToPrettyString(actor):player} {(args.Enabled ? "enabled" : "disabled")} auto-buy on gas miner console {ToPrettyString(ent)}");
    }

    private void OnBuyGasCredits(Entity<GasMinerConsoleComponent> ent, ref BuyGasCreditsMessage args)
    {
        if (args.Actor == null)
            return;

        var actor = args.Actor;

        // Access check
        if (!EntityManager.TryGetComponent<CargoOrderConsoleComponent>(ent, out var orderConsole) || !_accessReader.IsAllowed(actor, ent))
        {
            _popup.PopupEntity(Loc.GetString("cargo-console-order-not-allowed"), ent);
            _audio.PlayPredicted(orderConsole?.ErrorSound ?? default, ent, actor);
            return;
        }

        int specoAmount = args.Amount;

        // Attempt the purchase
        bool success = TryPurchaseGasCredits(ent, orderConsole, specoAmount, actor);

        if (!success)
        {
            _popup.PopupEntity(Loc.GetString("cargo-no-funds"), ent);
            _audio.PlayPredicted(orderConsole?.ErrorSound ?? default, ent, actor);
            return;
        }

        float gasCreditsAdded = specoAmount * 100f;

        _audio.PlayPvs(ApproveSound, ent);
        _popup.PopupEntity(Loc.GetString("gas-miner-purchase-success", ("amount", args.Amount), ("credits", gasCreditsAdded)), ent);

        _adminLog.Add(LogType.Action, LogImpact.Medium,
            $"{ToPrettyString(actor):player} purchased {gasCreditsAdded} gas mining credits for {args.Amount} spesos at {ToPrettyString(ent)}");

        var tryGetIdentity = new TryGetIdentityShortInfoEvent(ent, actor);
        RaiseLocalEvent(tryGetIdentity);

        var playerName = tryGetIdentity.Title ?? Loc.GetString("cargo-console-fund-transfer-user-unknown");
        var accountProto = _proto.Index(orderConsole.Account);

        var msg = Loc.GetString("gas-miner-purchase-broadcast",
            ("name", playerName),
            ("credits", gasCreditsAdded),
            ("amount", args.Amount),
            ("accountName", Loc.GetString(accountProto.Name)),
            ("accountCode", Loc.GetString(accountProto.Code)));

        _radio.SendRadioMessage(ent, msg, accountProto.RadioChannel, ent, escapeMarkup: false);
    }

    public bool TryPurchaseGasCredits(Entity<GasMinerConsoleComponent> ent, CargoOrderConsoleComponent orderConsole, int specoAmount, EntityUid? actor = null)
    {
        if (specoAmount <= 0)
            return false;

        // Get station & bank account
        var station = _station.GetOwningStation(ent);
        if (station == null || !TryComp<StationBankAccountComponent>(station, out var bank))
            return false;

        var accountId = orderConsole.Account;

        // Balance check
        var currentBalance = _cargo.GetBalanceFromAccount((station.Value, bank), accountId);
        if (currentBalance < specoAmount)
        {
            return false;
        }

        // Apply transaction
        _cargo.WithdrawFunds((station.Value, bank), -specoAmount, accountId);

        // Convert to gas credits
        float gasCreditsToAdd = specoAmount * 100f;
        ent.Comp.Credits += gasCreditsToAdd;
        Dirty(ent);

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

        if (HasComp<GasMinerComponent>(args.Sink))
        {
            SyncLinkedMiners(ent);
        }
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
        var sourceComp = Comp<DeviceLinkSourceComponent>(ent.Owner);

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
