// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._RMC14.Weapons.Ranged.Prediction;

[RegisterComponent]
public sealed partial class PredictedProjectileClientComponent : Component
{
    [DataField]
    public bool Hit;

    [DataField]
    public EntityCoordinates? Coordinates;
}
