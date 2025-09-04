// SPDX-FileCopyrightText: 2025 YourName
// SPDX-License-Identifier: MIT

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{

    /// <summary>
    /// The duration (in seconds) of the Malf AI Doomsday Protocol.
    /// </summary>
    public static readonly CVarDef<float> MalfAiDoomsdayDuration =
        CVarDef.Create("malfai.doomsday_duration", 420f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Duration in seconds that an APC remains siphoned after being targeted by a Malf AI.
    /// During this time, the APC is completely disabled and cannot supply power.
    /// </summary>
    public static readonly CVarDef<float> MalfAiSiphonDurationSeconds =
        CVarDef.Create("malfai.siphon_duration_seconds", 60f, CVar.SERVER | CVar.ARCHIVE);

    /// <summary>
    /// The amount of CPU that a Malf AI gains per APC siphon.
    /// </summary>
    public static readonly CVarDef<int> MalfAiSiphonCpuAmount =
        CVarDef.Create("malfai.siphon_cpu_amount", 5, CVar.SERVER | CVar.ARCHIVE);
}
