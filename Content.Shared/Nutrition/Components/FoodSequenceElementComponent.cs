// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Toaster <mrtoastymyroasty@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Nutrition.Prototypes;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared.Nutrition.Components;

/// <summary>
/// Indicates that this entity can be inserted into FoodSequence, which will transfer all reagents to the target.
/// </summary>
[RegisterComponent, Access(typeof(SharedFoodSequenceSystem))]
public sealed partial class FoodSequenceElementComponent : Component
{
    /// <summary>
    /// The same object can be used in different sequences, and it will have a different data in then.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<ProtoId<TagPrototype>, ProtoId<FoodSequenceElementPrototype>> Entries = new();

    /// <summary>
    /// Which solution we will add to the main dish
    /// </summary>
    [DataField]
    public string Solution = "food";
}
