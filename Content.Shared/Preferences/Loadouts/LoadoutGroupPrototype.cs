// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Shared.Preferences.Loadouts;

/// <summary>
/// Corresponds to a set of loadouts for a particular slot.
/// </summary>
[Prototype]
public sealed partial class LoadoutGroupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    /// <summary>
    /// User-friendly name for the group.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    /// Minimum number of loadouts that need to be specified for this category.
    /// </summary>
    [DataField]
    public int MinLimit = 1;

    /// <summary>
    /// Maximum limit for the category.
    /// </summary>
    [DataField]
    public int MaxLimit = 1;

    /// <summary>
    /// Hides the loadout group from the player.
    /// </summary>
    [DataField]
    public bool Hidden;

    [DataField(required: true)]
    public List<ProtoId<LoadoutPrototype>> Loadouts = new();
}
