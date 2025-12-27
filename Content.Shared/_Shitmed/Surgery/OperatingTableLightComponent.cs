// SPDX-FileCopyrightText: 2025 otokonoko-dev <248204705+otokonoko-dev@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Surgery;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class OperatingTableLightComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool LightOn = false;

    [DataField]
    public SoundSpecifier? HeartbeatHealthySound = new SoundPathSpecifier("/Audio/_Funkystation/Medical/heartbeat_monitor.ogg");

    [DataField]
    public SoundSpecifier? HeartbeatInjuredSound = new SoundPathSpecifier("/Audio/_Funkystation/Medical/heartbeat_monitor_injured.ogg");

    [DataField]
    public SoundSpecifier? HeartbeatCriticalSound = new SoundPathSpecifier("/Audio/_Funkystation/Medical/heartbeat_monitor_critical.ogg");

    [DataField]
    public SoundSpecifier? FlatlineSound = new SoundPathSpecifier("/Audio/_Funkystation/Medical/flatline.ogg");

    /// <summary>
    /// Currently playing heartbeat sound entity
    /// </summary>
    public EntityUid? HeartbeatStream;

    /// <summary>
    /// Currently playing flatline sound entity
    /// </summary>
    public EntityUid? FlatlineStream;

    /// <summary>
    /// The heartbeat sound that is currently playing (to avoid unnecessary restarts)
    /// </summary>
    public SoundSpecifier? CurrentHeartbeatSound;
}
