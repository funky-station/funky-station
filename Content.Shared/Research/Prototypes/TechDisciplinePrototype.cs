// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Kira Bridgeton <161087999+Verbalase@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PoTeletubby <ajcraigaz@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Research.Prototypes;

/// <summary>
/// This is a prototype for a research discipline, a category
/// that governs how <see cref="TechnologyPrototype"/>s are unlocked.
/// </summary>
[Prototype]
public sealed partial class TechDisciplinePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Player-facing name.
    /// Supports locale strings.
    /// </summary>
    [DataField("name", required: true)]
    public string Name = string.Empty;

    /// <summary>
    /// A color used for UI
    /// </summary>
    [DataField("color", required: true)]
    public Color Color;

    /// <summary>
    /// An icon used to visually represent the discipline in UI.
    /// </summary>
    [DataField("icon")]
    public SpriteSpecifier Icon = default!;

    /// <summary>
    /// For each tier a discipline supports, what percentage
    /// of the previous tier must be unlocked for it to become available
    /// </summary>
    [DataField("tierPrerequisites", required: true)]
    public Dictionary<int, float> TierPrerequisites = new();

    /// <summary>
    /// Purchasing this tier of technology causes a server to become "locked" to this discipline.
    /// </summary>
    [DataField("lockoutTier")]
    public int LockoutTier = 3;
}
