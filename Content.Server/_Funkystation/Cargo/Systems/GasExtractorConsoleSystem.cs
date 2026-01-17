// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Funkystation.Cargo.Components;
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
using Robust.Shared.Audio;
using Content.Shared.Cargo;
using Content.Server.Radio.EntitySystems;
using Content.Shared.IdentityManagement;
using Robust.Shared.Prototypes;
using Content.Shared.Atmos.Prototypes;
using Content.Shared._Funkystation.Atmos.Components;
using Robust.Shared.Configuration;
using Content.Shared._Funkystation.CCVars;

namespace Content.Server._Funkystation.Cargo.Systems;

public sealed partial class GasExtractorConsoleSystem : SharedCargoSystem
{
    [Dependency] private readonly CargoSystem _cargo = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    private static readonly SoundPathSpecifier ApproveSound = new("/Audio/Effects/Cargo/ping.ogg");

    public override void Initialize()
    {
        SubscribeLocalEvent<GasExtractorConsoleComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GasExtractorConsoleComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<CargoOrderConsoleComponent, GasExtractorSetSettingsMessage>(OnSetSettings);
        SubscribeLocalEvent<CargoOrderConsoleComponent, BuyMolesForExtractorMessage>(OnBuyMolesForExtractor);
        SubscribeLocalEvent<CargoOrderConsoleComponent, ToggleAutoBuyExtractorMessage>(OnToggleAutoBuyExtractor);
    }

    private void OnToggleAutoBuyExtractor(Entity<CargoOrderConsoleComponent> ent, ref ToggleAutoBuyExtractorMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        if (!TryComp<GasExtractorConsoleComponent>(ent, out var gasConsole))
            return;

        if (args.ExtractorIndex < 0 || args.ExtractorIndex >= gasConsole.LinkedExtractors.Count)
            return;

        var extractorUid = gasConsole.LinkedExtractors[args.ExtractorIndex];
        if (!TryComp<GasExtractorComponent>(extractorUid, out var extractor))
            return;

        // Access check
        if (!_accessReader.IsAllowed(actor, ent.Owner))
        {
            _popup.PopupEntity(Loc.GetString("cargo-console-order-not-allowed"), actor, actor);
            _audio.PlayPredicted(ent.Comp.ErrorSound ?? default, ent, actor);
            return;
        }

        extractor.AutoBuyEnabled = args.Enabled;
        Dirty(extractorUid, extractor);

        var tryGetIdentity = new TryGetIdentityShortInfoEvent(ent, actor);
        RaiseLocalEvent(tryGetIdentity);

        var playerName = tryGetIdentity.Title ?? Loc.GetString("cargo-console-fund-transfer-user-unknown");
        var accountProto = _proto.Index(ent.Comp.Account);

        var key = args.Enabled
            ? "gas-extractor-extractor-autobuy-enabled"
            : "gas-extractor-extractor-autobuy-disabled";

        var msg = Loc.GetString(key,
            ("name", playerName),
            ("gas", Loc.GetString(_proto.Index<GasPrototype>(((int)extractor.SpawnGas).ToString()).Name)));

        _radio.SendRadioMessage(ent, msg, accountProto.RadioChannel, ent, escapeMarkup: false);

        _adminLog.Add(LogType.Action, LogImpact.Low,
            $"{ToPrettyString(actor):player} {(args.Enabled ? "enabled" : "disabled")} auto-buy on extractor {ToPrettyString(extractorUid)} (gas: {extractor.SpawnGas}) at console {ToPrettyString(ent)}");
    }

    private void OnBuyMolesForExtractor(Entity<CargoOrderConsoleComponent> ent, ref BuyMolesForExtractorMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        if (!TryComp<GasExtractorConsoleComponent>(ent, out var gasConsole))
            return;

        if (args.ExtractorIndex < 0 || args.ExtractorIndex >= gasConsole.LinkedExtractors.Count)
            return;

        var extractorUid = gasConsole.LinkedExtractors[args.ExtractorIndex];
        if (!TryComp<GasExtractorComponent>(extractorUid, out var extractor))
            return;

        // Access check
        if (!_accessReader.IsAllowed(actor, ent.Owner))
        {
            _popup.PopupEntity(Loc.GetString("cargo-console-order-not-allowed"), actor, actor);
            _audio.PlayPredicted(ent.Comp.ErrorSound ?? default, ent, actor);
            return;
        }

        int spesoCost = args.SpecoAmount;
        if (spesoCost <= 0)
            return;

        var station = _station.GetOwningStation(ent);
        if (station == null || !TryComp<StationBankAccountComponent>(station, out var bank))
        {
            return;
        }

        var accountId = ent.Comp.Account;
        var currentBalance = _cargo.GetBalanceFromAccount((station.Value, bank), accountId);
        if (currentBalance < spesoCost)
        {
            _popup.PopupEntity(Loc.GetString("cargo-console-insufficient-funds", ("cost", spesoCost)), actor, actor);
            _audio.PlayPredicted(ent.Comp.ErrorSound ?? default, ent, actor);
            return;
        }

        string gasId = ((int)extractor.SpawnGas).ToString();
        if (!_proto.TryIndex<GasPrototype>(gasId, out var gasProto))
        {
            _popup.PopupEntity("Invalid gas prototype!", actor, actor);
            return;
        }

        float molesToAdd;

        if (gasProto.PricePerMole <= 0f || gasConsole.PriceMultiplier <= 0f || !_cfg.GetCVar(CCVars_Funky.GasExtractorsRequirePayment))
        {
            molesToAdd = 100000f; // Arbitrary large number for free purchases
            spesoCost = 0;
        }
        else
        {
            float costPerMole = gasProto.PricePerMole * gasConsole.PriceMultiplier;
            molesToAdd = spesoCost / costPerMole;
        }

        // Apply transaction
        if (spesoCost > 0)
        {
            _cargo.WithdrawFunds((station.Value, bank), -spesoCost, accountId);
        }

        extractor.RemainingMoles += molesToAdd;
        Dirty(extractorUid, extractor);

        _audio.PlayPvs(ApproveSound, ent);
        _popup.PopupEntity(Loc.GetString("gas-extractor-moles-purchase-success",
            ("moles", molesToAdd.ToString("F1")),
            ("gas", Loc.GetString(gasProto.Name)),
            ("spesos", spesoCost)), actor, actor);

        _adminLog.Add(LogType.Action, LogImpact.Medium,
            $"{ToPrettyString(actor):player} purchased {molesToAdd:F1} moles of {extractor.SpawnGas} for {spesoCost} spesos at console {ToPrettyString(ent)} (extractor: {ToPrettyString(extractorUid)})");

        var tryGetIdentity = new TryGetIdentityShortInfoEvent(ent, actor);
        RaiseLocalEvent(tryGetIdentity);

        var playerName = tryGetIdentity.Title ?? Loc.GetString("cargo-console-fund-transfer-user-unknown");
        var accountProto = _proto.Index(accountId);

        var msg = Loc.GetString("gas-extractor-moles-purchase-broadcast",
            ("name", playerName),
            ("moles", molesToAdd.ToString("F1")),
            ("gas", Loc.GetString(gasProto.Name)),
            ("spesos", spesoCost),
            ("accountName", Loc.GetString(accountProto.Name)),
            ("accountCode", Loc.GetString(accountProto.Code)));

        _radio.SendRadioMessage(ent, msg, accountProto.RadioChannel, ent, escapeMarkup: false);
    }

    public bool TryAutoPurchaseMoles(EntityUid consoleUid, CargoOrderConsoleComponent orderConsole, EntityUid extractorUid, float desiredMoles)
    {
        if (!TryComp<GasExtractorComponent>(extractorUid, out var extractor))
            return false;

        if (!TryComp<GasExtractorConsoleComponent>(consoleUid, out var gasConsole))
            return false;

        string gasId = ((int)extractor.SpawnGas).ToString();
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

        // Add moles directly to this extractor
        extractor.RemainingMoles += desiredMoles;
        Dirty(extractorUid, extractor);

        return true;
    }

    private void OnStartup(Entity<GasExtractorConsoleComponent> ent, ref ComponentStartup args)
    {
        SyncLinkedExtractors(ent);
    }

    private void OnNewLink(Entity<GasExtractorConsoleComponent> ent, ref NewLinkEvent args)
    {
        if (args.Source != ent.Owner)
            return;

        if (!TryComp<DeviceLinkSourceComponent>(ent.Owner, out _))
            return;

        if (HasComp<GasExtractorComponent>(args.Sink))
            SyncLinkedExtractors(ent);
    }

    private void OnSetSettings(Entity<CargoOrderConsoleComponent> ent, ref GasExtractorSetSettingsMessage args)
    {
        if (!TryComp<GasExtractorConsoleComponent>(ent, out var gasConsole))
            return;

        if (args.ExtractorIndex < 0 || args.ExtractorIndex >= gasConsole.LinkedExtractors.Count)
            return;

        var extractorUid = gasConsole.LinkedExtractors[args.ExtractorIndex];

        if (!TryComp<GasExtractorComponent>(extractorUid, out var extractor))
            return;

        extractor.SpawnAmount = args.NewSpawnAmount;
        extractor.MaxExternalPressure = args.NewMaxExternalPressure;

        Dirty(extractorUid, extractor);
    }

    private void SyncLinkedExtractors(Entity<GasExtractorConsoleComponent> ent)
    {
        if (!TryComp<DeviceLinkSourceComponent>(ent.Owner, out var sourceComp))
            return;

        var newExtractors = sourceComp.LinkedPorts.Keys
            .Where(sink => HasComp<GasExtractorComponent>(sink))
            .ToList();

        var comp = ent.Comp;

        if (!comp.LinkedExtractors.SequenceEqual(newExtractors))
        {
            comp.LinkedExtractors.Clear();
            comp.LinkedExtractors.AddRange(newExtractors);
            Dirty(ent, comp);
        }
    }
}
