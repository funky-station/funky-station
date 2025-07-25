// SPDX-FileCopyrightText: 2025 IronDragoon <8961391+IronDragoon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;

namespace Content.Shared._DV.TapeRecorder.Components;

/// <summary>
/// Added to tape records that are updating, winding or rewinding the tape.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ActiveTapeRecorderComponent : Component;
