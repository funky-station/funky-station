// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Access;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Turrets;

/// <summary>
/// Attached to entities to provide them with turret target selection data.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(TurretTargetSettingsSystem))]
public sealed partial class TurretTargetSettingsComponent : Component
{
    /// <summary>
    /// Crew with one or more access levels from this list are exempt from being targeted by turrets.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<AccessLevelPrototype>> ExemptAccessLevels = new();
}
