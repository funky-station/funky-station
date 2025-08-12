// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Parallax;

/// <summary>
/// Handles per-map parallax in sim. Out of sim parallax is handled by ParallaxManager.
/// </summary>
public abstract class SharedParallaxSystem: EntitySystem
{
    [Serializable, NetSerializable]
    protected sealed class ParallaxComponentState : ComponentState
    {
        public string Parallax = string.Empty;
    }
}
