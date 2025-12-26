// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.PowerCell.Components;
using Content.Shared._EinsteinEngines.Silicon;
using Content.Shared.Verbs;
using Robust.Shared.Utility;
using Content.Server._EinsteinEngines.Silicon.Charge;
using Content.Shared._EinsteinEngines.Silicon.Charge;
using Content.Server.Popups;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Content.Shared._EinsteinEngines.Power.Components;
using Content.Shared._EinsteinEngines.Power.Systems;
using Content.Shared.Power.Components;
using Content.Shared.PowerCell;
using Content.Shared.Whitelist;
using Content.Server.Power.EntitySystems;
using Content.Shared.Power.EntitySystems;

namespace Content.Server._EinsteinEngines.Power;

public sealed class BatteryDrinkerSystem : SharedBatteryDrinkerSystem
{
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!; // UPDATED
    [Dependency] private readonly SiliconChargeSystem _silicon = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly ChargerSystem _chargers = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryComponent, GetVerbsEvent<AlternativeVerb>>(AddAltVerb);
        SubscribeLocalEvent<PowerCellSlotComponent, GetVerbsEvent<AlternativeVerb>>(AddAltVerb);
        SubscribeLocalEvent<BatteryDrinkerComponent, BatteryDrinkerDoAfterEvent>(OnDoAfter);
    }

    private void AddAltVerb<TComp>(EntityUid uid, TComp component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!TryComp<BatteryDrinkerComponent>(args.User, out var drinkerComp) ||
            _whitelist.IsWhitelistPass(drinkerComp.Blacklist, uid) ||
            !SearchForDrinker(args.User, out _) ||
            !SearchForSource(uid, out var batteryEnt) ||
            !TestDrinkableBattery(batteryEnt.Value, drinkerComp))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Act = () => DrinkBattery(batteryEnt.Value, args.User, drinkerComp),
            Text = Loc.GetString("battery-drinker-verb-drink"),
            Icon = new SpriteSpecifier.Texture(
                new ResPath("/Textures/Interface/VerbIcons/smite.svg.192dpi.png")),
            Priority = -5
        });
    }

    private bool TestDrinkableBattery(EntityUid target, BatteryDrinkerComponent drinkerComp)
    {
        return HasComp<BatteryDrinkerSourceComponent>(target);
    }

    private void DrinkBattery(EntityUid target, EntityUid user, BatteryDrinkerComponent drinkerComp)
    {
        if (!TryComp<BatteryDrinkerSourceComponent>(target, out var sourceComp))
            return;

        var doAfterTime = drinkerComp.DrinkSpeed * sourceComp.DrinkSpeedMulti;

        var args = new DoAfterArgs(
            EntityManager,
            user,
            doAfterTime,
            new BatteryDrinkerDoAfterEvent(),
            user,
            target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 1.35f,
            RequireCanInteract = true,
            CancelDuplicate = false
        };

        _doAfter.TryStartDoAfter(args);
    }

    private void OnDoAfter(EntityUid uid, BatteryDrinkerComponent drinkerComp, DoAfterEvent args)
    {
        if (args.Cancelled || args.Target == null)
            return;

        var source = args.Target.Value;
        var drinker = uid;

        if (!TryComp<BatteryComponent>(source, out var sourceBattery))
            return;

        if (!SearchForDrinker(drinker, out var drinkerBatteryEnt)
            || !TryComp<BatteryComponent>(drinkerBatteryEnt, out var drinkerBattery))
            return;

        TryComp<BatteryDrinkerSourceComponent>(source, out var sourceComp);

        var sourceBatteryEntity = new Entity<BatteryComponent?>(source, sourceBattery);
        var drinkerBatteryEntity = new Entity<BatteryComponent?>(drinkerBatteryEnt!.Value, drinkerBattery);

        var sourceCharge = _battery.GetCharge(sourceBatteryEntity);
        var drinkerCharge = _battery.GetCharge(drinkerBatteryEntity);

        var amountToDrink = drinkerComp.DrinkMultiplier * 1000f;

        amountToDrink = MathF.Min(amountToDrink, sourceCharge);
        amountToDrink = MathF.Min(amountToDrink, drinkerBattery.MaxCharge - drinkerCharge);

        _battery.TryUseCharge(sourceBatteryEntity, amountToDrink);
        _battery.SetCharge(drinkerBatteryEntity, drinkerCharge + amountToDrink);

        if (sourceComp?.DrinkSound != null)
        {
            _popup.PopupEntity(
                Loc.GetString("ipc-recharge-tip"),
                drinker,
                drinker,
                PopupType.SmallCaution);

            _audio.PlayPvs(sourceComp.DrinkSound, source);
            Spawn("EffectSparks", Transform(source).Coordinates);
        }
    }
}
