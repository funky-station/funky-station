// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Toaster <mrtoastymyroasty@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._RMC14.CCVar;

[CVarDefs]
public sealed class RMCCVars : CVars
{
    public static readonly CVarDef<bool> RMCGunPrediction =
        CVarDef.Create("rmc.gun_prediction", true, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<bool> RMCGunPredictionPreventCollision =
        CVarDef.Create("rmc.gun_prediction_prevent_collision", false, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<bool> RMCGunPredictionLogHits =
        CVarDef.Create("rmc.gun_prediction_log_hits", false, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float> RMCGunPredictionCoordinateDeviation =
        CVarDef.Create("rmc.gun_prediction_coordinate_deviation", 0.75f, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float> RMCGunPredictionLowestCoordinateDeviation =
        CVarDef.Create("rmc.gun_prediction_lowest_coordinate_deviation", 0.5f, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float> RMCGunPredictionAabbEnlargement =
        CVarDef.Create("rmc.gun_prediction_aabb_enlargement", 0.3f, CVar.SERVER | CVar.REPLICATED);
}
