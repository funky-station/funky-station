// SPDX-FileCopyrightText: 2025 8tv <eightev@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Overlays;

/// <summary>
/// Makes the entity see everything in black and white by adding an overlay.
/// When added to a clothing item it will also grant the wearer the same overlay.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BlackAndWhiteOverlayComponent : Component;
