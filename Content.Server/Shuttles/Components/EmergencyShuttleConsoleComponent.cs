// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Shuttles.Components;

[RegisterComponent]
public sealed partial class EmergencyShuttleConsoleComponent : Component
{
    // TODO: Okay doing it by string is kinda suss but also ID card tracking doesn't seem to be robust enough

    /// <summary>
    /// ID cards that have been used to authorize an early launch.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("authorized")]
    public HashSet<string> AuthorizedEntities = new();

    [ViewVariables(VVAccess.ReadWrite), DataField("authorizationsRequired")]
    public int AuthorizationsRequired = 3;
}
