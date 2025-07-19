// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 TakoDragon <69509841+BackeTako@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Speech.EntitySystems;

namespace Content.Server.Speech.Components;

/// <summary>
/// French accent replaces spoken letters. "th" becomes "z" and "H" at the start of a word becomes "'".
/// </summary>
[RegisterComponent]
[Access(typeof(FrenchAccentSystem))]
public sealed partial class FrenchAccentComponent : Component {}
