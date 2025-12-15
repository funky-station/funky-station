// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using System;

namespace Content.Shared.XRay;

[RegisterComponent, NetworkedComponent] // No AutoGenerateComponentState
public sealed partial class ShowXRayComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField(required: true)]
    public string Shader = string.Empty;

    [DataField]
    public float EntityRange = 8;

    [DataField]
    public float TileRange = 9;
}

// Define the data packet that travels over the network
[Serializable, NetSerializable]
public sealed class ShowXRayComponentState : ComponentState
{
    public bool Enabled;
    public string Shader;
    public float EntityRange;
    public float TileRange;

    public ShowXRayComponentState(bool enabled, string shader, float entityRange, float tileRange)
    {
        Enabled = enabled;
        Shader = shader;
        EntityRange = entityRange;
        TileRange = tileRange;
    }
}
