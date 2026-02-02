// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.GameObjects;
using Content.Shared.Atmos.Visuals;

namespace Content.Client._Funkystation.Atmos.Visualizers;

[RegisterComponent]
public sealed partial class PipeScrubberVisualsComponent : Component
{
    [DataField("idleState", required: true)]
    public string IdleState = default!;

    [DataField("enabledState", required: true)]
    public string EnabledState = default!;

    [DataField("fullState", required: true)]
    public string FullState = default!;

    [DataField("scrubbingState", required: true)]
    public string ScrubbingState = default!;

    [DataField("drainingState", required: true)]
    public string DrainingState = default!;
}
