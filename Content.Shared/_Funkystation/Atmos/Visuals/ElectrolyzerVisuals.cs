// SPDX-FileCopyrightText: 2024 Menshin <Menshin@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Atmos.Visuals;

/// <summary>
///     Assmos - /tg/ gases
///     Used for the visualizer
/// </summary>
[Serializable, NetSerializable]
public enum ElectrolyzerVisualLayers : byte
{
    Main
}

[Serializable, NetSerializable]
public enum ElectrolyzerVisuals : byte
{
    State,
}

[Serializable, NetSerializable]
public enum ElectrolyzerState : byte
{
    Off,
    On,
}
