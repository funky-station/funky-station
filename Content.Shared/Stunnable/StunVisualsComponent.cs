// SPDX-FileCopyrightText: 2025 Drywink <hugogrethen@gmail.com>
// SPDX-FileCopyrightText: 2025 Princess Cheeseballs <66055347+princess-cheeseballs@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Princess-Cheeseballs <https://github.com/Princess-Cheeseballs>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Utility;

namespace Content.Shared.Stunnable;

/// <summary>
/// This is used to listen to incoming events from the AppearanceSystem
/// </summary>
[RegisterComponent]
public sealed partial class StunVisualsComponent : Component
{
    [DataField]
    public ResPath StarsPath = new ("Mobs/Effects/stunned.rsi");

    [DataField]
    public string State = "stunned";
}
