// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Antag;

/// <summary>
/// Determines whether Someone can see antags icons
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShowAntagIconsComponent: Component;
