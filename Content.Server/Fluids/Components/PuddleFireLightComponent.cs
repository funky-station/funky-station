// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Fluids.Components
{
    [RegisterComponent]
    public sealed partial class PuddleFireLightComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan ExtinguishTime;

        [ViewVariables]
        public EntityUid? LightEntity;
    }
}
