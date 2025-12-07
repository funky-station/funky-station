// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.UserInterface.Fragments;
using Content.Shared.Mech;
using Content.Shared.Mech.Components;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.Mech.Ui;

[UsedImplicitly]
public sealed class MechBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private MechMenu? _menu;

    public MechBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindowCenteredLeft<MechMenu>();
        _menu.SetEntity(Owner);

        _menu.OnRemoveButtonPressed += uid =>
        {
            SendMessage(new MechEquipmentRemoveMessage(EntMan.GetNetEntity(uid)));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not MechBoundUiState msg)
            return;
        UpdateEquipmentControls(msg);
        _menu?.UpdateMechStats();
        _menu?.UpdateEquipmentView();
    }

    public void UpdateEquipmentControls(MechBoundUiState state)
    {
        if (!EntMan.TryGetComponent<MechComponent>(Owner, out var mechComp))
            return;

        foreach (var ent in mechComp.EquipmentContainer.ContainedEntities)
        {
            var ui = GetEquipmentUi(ent);
            if (ui == null)
                continue;
            foreach (var (attached, estate) in state.EquipmentStates)
            {
                if (ent == EntMan.GetEntity(attached))
                    ui.UpdateState(estate);
            }
        }
    }

    public UIFragment? GetEquipmentUi(EntityUid? uid)
    {
        var component = EntMan.GetComponentOrNull<UIFragmentComponent>(uid);
        component?.Ui?.Setup(this, uid);
        return component?.Ui;
    }
}

