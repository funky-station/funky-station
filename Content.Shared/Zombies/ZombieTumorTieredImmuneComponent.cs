// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Zombies;

/// <summary>
/// Makes an entity immune to zombie tumor infections at a specific tier level (1-5).
/// Level 1: Immunity from Incubation only (airborne)
/// Level 2: Immunity from Incubation + Early stages
/// Level 3: Immunity from Incubation + Early + TumorFormed
/// Level 4: Immunity from all stages up to Advanced
/// Level 5: Complete immunity (all infection methods and stages)
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ZombieTumorTieredImmuneComponent : Component
{
    /// <summary>
    /// Immunity level (1-5). Higher levels provide protection from more infection stages.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int ImmunityLevel = 1;

    /// <summary>
    /// When the immunity expires. After this time, the component will be removed.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan ExpiresAt;
}
