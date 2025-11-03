// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared._RMC14.Weapons.Ranged.Prediction;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedGunPredictionSystem))]
public sealed partial class IgnorePredictionHideComponent : Component;
