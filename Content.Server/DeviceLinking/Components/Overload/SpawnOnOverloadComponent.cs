// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.DeviceLinking.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.DeviceLinking.Components.Overload;

/// <summary>
/// Spawns an entity when a device link overloads.
/// An overload happens when a device link sink is invoked to many times per tick
/// and it raises a <see cref="Content.Shared.DeviceLinking.Events.DeviceLinkOverloadedEvent"/>
/// </summary>
[RegisterComponent]
[Access(typeof(DeviceLinkOverloadSystem))]
public sealed partial class SpawnOnOverloadComponent : Component
{
    /// <summary>
    /// The entity prototype to spawn when the device overloads
    /// </summary>
    [DataField("spawnedPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Prototype = "PuddleSparkle";
}
