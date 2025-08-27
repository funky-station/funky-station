// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Conveyor;

/// <summary>
/// Indicates this entity is currently contacting a conveyor and will subscribe to events as appropriate.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ConveyedComponent : Component
{
    // TODO: Delete if pulling gets fixed.
    /// <summary>
    /// True if currently conveying.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Conveying;
}
