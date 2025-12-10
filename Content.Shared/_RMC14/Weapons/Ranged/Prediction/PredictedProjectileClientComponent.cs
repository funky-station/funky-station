// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Toaster <mrtoastymyroasty@gmail.com>
// SPDX-FileCopyrightText: 2025 Toastermeister <215405651+Toastermeister@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.FixedPoint;
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

    [DataField]
    public bool IgnoreShooter = true;

    [DataField]
    public bool DeleteOnCollide = true;

    [DataField]
    public bool OnlyCollideWhenShot = false;

    [DataField]
    public bool DamagedEntity;

    [DataField]
    public bool ProjectileSpent;

    [DataField]
    public FixedPoint2 PenetrationThreshold = FixedPoint2.Zero;

    [DataField]
    public List<string>? PenetrationDamageTypeRequirement;

    [DataField]
    public FixedPoint2 PenetrationAmount = FixedPoint2.Zero;
}
