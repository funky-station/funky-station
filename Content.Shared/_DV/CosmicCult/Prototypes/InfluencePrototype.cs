// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._DV.CosmicCult.Prototypes;

/// <summary>
/// An influence that can be purchased from the monument
/// </summary>
[Prototype]
public sealed partial class InfluencePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name;

    [DataField(required: true)]
    public LocId InfluenceType;

    [DataField(required: true)]
    public int Cost;

    [DataField(required: true)]
    public LocId Description;

    [DataField(required: true)]
    public SpriteSpecifier Icon = SpriteSpecifier.Invalid;

    [DataField]
    public EntProtoId? Action;

    [DataField]
    public ComponentRegistry? Add;

    [DataField]
    public ComponentRegistry? Remove;

    [DataField(required: true)]
    public int Tier;
}
