// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Stalen <33173619+stalengd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Clothing.EntitySystems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Clothing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MaskSystem))]
public sealed partial class MaskComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ToggleAction = "ActionToggleMask";

    /// <summary>
    /// This mask can be toggled (pulled up/down)
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;

    [DataField, AutoNetworkedField]
    public bool IsToggled;

    /// <summary>
    /// Equipped prefix to use after the mask was pulled down.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string EquippedPrefix = "up";

    /// <summary>
    /// When <see langword="true"/> will function normally, otherwise will not react to events
    /// </summary>
    [DataField("enabled"), AutoNetworkedField]
    public bool IsEnabled = true;

    /// <summary>
    /// When <see langword="true"/> will disable <see cref="IsEnabled"/> when folded
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool DisableOnFolded;
}
