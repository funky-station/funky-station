// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Pinpointer;

[RegisterComponent]
public sealed partial class StationMapComponent : Component
{
    /// <summary>
    /// Whether or not to show the user's location on the map.
    /// </summary>
    [DataField]
    public bool ShowLocation = true;
}
