// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Atmos.Components
{
    [RegisterComponent]
    public sealed partial class GridFireLightsComponent : Component
    {
        public Dictionary<Vector2i, EntityUid> ActiveLights = new();
    }
}
