// SPDX-FileCopyrightText: 2025 Amethyst <52829582+jackel234@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Tobias Berger <toby@tobot.dev>
// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 jackel234 <jackel234@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Holosign;

/// <summary>
/// A holosign projector that uses <c>LimitedCharges</c> instead of a power cell slot.
/// If there is already a sign on the clicked tile it reclaims it for a charge instead of stacking it.
/// Currently there is no spawning prediction so signs are spawned once in a container and moved out to allow prediction.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), Access(typeof(ChargeHolosignSystem))]
public sealed partial class ChargeHolosignProjectorComponent : Component
{
    /// <summary>
    /// The entity to spawn.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId SignProto;

    /// <summary>
    /// Component on <see cref="SignProto"/> to check for duplicates.
    /// </summary>
    [DataField(required: true)]
    public string SignComponentName;

    public Type SignComponent = default!;

    /// <summary>
    /// Container to store sign entities in before they are "spawned" on use.
    /// </summary>
    [DataField]
    public string ContainerId = "signs";

    /// <summary>
    /// Holosigns we "own".
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public List<EntityUid> Signs = new();

    [ViewVariables]
    public Container Container = default!;
}
