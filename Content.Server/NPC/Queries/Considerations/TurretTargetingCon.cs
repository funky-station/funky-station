// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.NPC.Queries.Considerations;

/// <summary>
/// Returns 0f if the NPC has a <see cref="TurretTargetSettingsComponent"/> and the
/// target entity is exempt from being targeted, otherwise it returns 1f.
/// See <see cref="TurretTargetSettingsSystem.EntityIsTargetForTurret"/>
/// for further details on turret target validation.
/// </summary>
public sealed partial class TurretTargetingCon : UtilityConsideration
{

}
