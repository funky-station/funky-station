// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DVDPlayerOfDiscordFame <drew.c.bergen@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Shared.Damage.Components;


/// <summary>
/// This component is added to entities to protect them from being damaged
/// when shooting guns with the <see cref="DamageOnShootComponent"/>
/// ie. wearing a specific hardsuit can enable you to fire a Big, Powerful Gun that would normally Knock You Over.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DamageOnShootProtectionComponent : Component, IClothingSlots
{
    /// <summary>
    /// How much and what kind of damage to protect the user from
    /// when interacting with something with <see cref="DamageOnInteractComponent"/>
    /// </summary>
    [DataField(required: true)]
    public DamageModifierSet DamageProtection = default!;

    /// <summary>
    /// Only protects if the item is in the correct slot
    /// i.e. having gloves in your pocket doesn't protect you, it has to be on your hands
    /// </summary>
    [DataField]
    public SlotFlags Slots { get; set; } = SlotFlags.GLOVES;
}
