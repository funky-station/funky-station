// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 stellar-novas <stellar_novas@riseup.net>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Perry Fraser <perryprog@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.PowerCell.Components;

namespace Content.Shared.PowerCell;

/// <summary>
/// Handles events to integrate PowerCellDraw with ItemToggle
/// </summary>
public sealed class ToggleCellDrawSystem : EntitySystem
{
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedPowerCellSystem _cell = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ToggleCellDrawComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ToggleCellDrawComponent, ItemToggleActivateAttemptEvent>(OnActivateAttempt);
        SubscribeLocalEvent<ToggleCellDrawComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<ToggleCellDrawComponent, PowerCellSlotEmptyEvent>(OnEmpty);
    }

    private void OnMapInit(Entity<ToggleCellDrawComponent> ent, ref MapInitEvent args)
    {
        _cell.SetDrawEnabled(ent.Owner, _toggle.IsActivated(ent.Owner));
    }

    private void OnActivateAttempt(Entity<ToggleCellDrawComponent> ent, ref ItemToggleActivateAttemptEvent args)
    {
        if (!_cell.HasDrawCharge(ent, user: args.User)
            || !_cell.HasActivatableCharge(ent, user: args.User))
            args.Cancelled = true;
    }

    private void OnToggled(Entity<ToggleCellDrawComponent> ent, ref ItemToggledEvent args)
    {
        var uid = ent.Owner;
        var draw = Comp<PowerCellDrawComponent>(uid);
        _cell.SetDrawEnabled((uid, draw), args.Activated);
    }

    private void OnEmpty(Entity<ToggleCellDrawComponent> ent, ref PowerCellSlotEmptyEvent args)
    {
        _toggle.TryDeactivate(ent.Owner);
    }
}
