// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Mining.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(MiningScannerSystem))]
public sealed partial class MiningScannerViewableComponent : Component;

[Serializable, NetSerializable]
public enum MiningScannerVisualLayers : byte
{
    Overlay
}
