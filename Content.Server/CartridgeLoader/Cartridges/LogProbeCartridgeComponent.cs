// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Skubman <ba.fallaria@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared._DV.CartridgeLoader.Cartridges; // DeltaV
using Robust.Shared.Audio;

namespace Content.Server.CartridgeLoader.Cartridges;

[RegisterComponent]
[Access(typeof(LogProbeCartridgeSystem))]
public sealed partial class LogProbeCartridgeComponent : Component
{
    /// <summary>
    /// The list of pulled access logs
    /// </summary>
    [DataField, ViewVariables]
    public List<PulledAccessLog> PulledAccessLogs = new();

    /// <summary>
    /// The sound to make when we scan something with access
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier SoundScan = new SoundPathSpecifier("/Audio/Machines/scan_finish.ogg");

    /// <summary>
    /// DeltaV: The last scanned NanoChat data, if any
    /// </summary>
    [DataField]
    public NanoChatData? ScannedNanoChatData;
}
