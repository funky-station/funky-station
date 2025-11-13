// SPDX-FileCopyrightText: 2025 No Elka <125199100+NoElkaTheGod@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 No Elka <no.elka.the.god@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Indicates that an entity will be transmuted to the given prototype by using a specific glyph
/// </summary>
[RegisterComponent]
public sealed partial class CosmicTransmutableComponent : Component
{
    [DataField(required: true)]
    public EntProtoId? TransmutesTo;

    [DataField(required: true)]
    public EntProtoId? RequiredGlyphType;
}
