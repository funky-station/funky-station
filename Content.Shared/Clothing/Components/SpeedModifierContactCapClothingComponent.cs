// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Clothing.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// When equipped, sets a max cap to the slowdown applied from contact speed modifiers. (E.g. glue puddles, kudzu).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SpeedModifierContactCapClothingSystem))]
public sealed partial class SpeedModifierContactCapClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public float MaxContactSprintSlowdown = 1f;

    [DataField, AutoNetworkedField]
    public float MaxContactWalkSlowdown = 1f;
}
