// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Emag.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Emag.Components;

/// <summary>
/// Marker component for emagged entities
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EmaggedComponent : Component
{
    /// <summary>
    /// The EmagType flags that were used to emag this device
    /// </summary>
    [DataField, AutoNetworkedField]
    public EmagType EmagType = EmagType.None;
}
