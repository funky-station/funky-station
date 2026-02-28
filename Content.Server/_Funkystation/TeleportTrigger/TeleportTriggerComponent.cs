// SPDX-FileCopyrightText: 2026 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.TeleportTrigger;

[RegisterComponent]
public sealed partial class TeleportOnTriggerComponent : Component
{
    [DataField]
    public EntProtoId MarkerPrototype = "LifelineMarker";

    [DataField("allowNukeDisk")]
    public bool AllowNukeDisk = false;
}
