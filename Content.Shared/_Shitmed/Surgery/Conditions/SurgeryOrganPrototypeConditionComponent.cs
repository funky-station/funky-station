// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

/// <summary>
/// Condition that checks if an organ with a specific prototype ID is present in the body part.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryOrganPrototypeConditionComponent : Component
{
    /// <summary>
    /// The prototype ID of the organ that must be present (or absent if Inverse is true).
    /// </summary>
    [DataField]
    public ProtoId<EntityPrototype>? PrototypeId;

    /// <summary>
    /// If true, the surgery is valid when the organ is NOT present.
    /// </summary>
    [DataField]
    public bool Inverse = false;
}

