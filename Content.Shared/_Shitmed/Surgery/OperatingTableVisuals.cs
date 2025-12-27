// SPDX-FileCopyrightText: 2025 otokonoko-dev <248204705+otokonoko-dev@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Surgery;

[Serializable, NetSerializable]
public enum OperatingTableVisuals : byte
{
    LightOn,
    VitalsState,
}

[Serializable, NetSerializable]
public enum OperatingTableVisualLayers : byte
{
    Base,
    Vitals,
}

[Serializable, NetSerializable]
public enum VitalsState : byte
{
    None,
    Healthy,
    Injured,
    Critical,
    Dead,
}
