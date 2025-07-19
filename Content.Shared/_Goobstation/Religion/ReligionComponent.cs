// SPDX-FileCopyrightText: 2024 Dae <60460608+ZeroDayDaemon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

// Public Domain Code Begins

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Religion;

/// <summary>
///     Used for religions. Systems will check for this component and its fields in order to determine if religion based
///     interactions occur.
/// </summary>
[RegisterComponent] [NetworkedComponent] [AutoGenerateComponentState]
public sealed partial class ReligionComponent : Component
{
    /// <summary>
    ///     The religion of the entity
    /// </summary>
    [DataField("religion")] [AutoNetworkedField]
    public Religion Type = Religion.None;
}

public enum Religion
{
    None,
    Atheist,
    Buddhist,
    Christian,
}
// Public Domain Code Ends
