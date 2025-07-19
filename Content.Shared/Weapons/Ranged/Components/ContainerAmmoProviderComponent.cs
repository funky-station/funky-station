// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
///     Handles pulling entities from the given container to use as ammunition.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedGunSystem))]
public sealed partial class ContainerAmmoProviderComponent : AmmoProviderComponent
{
    [DataField("container", required: true)]
    [ViewVariables]
    public string Container = default!;

    [DataField("provider")]
    [ViewVariables]
    public EntityUid? ProviderUid;
}
