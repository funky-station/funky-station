// SPDX-FileCopyrightText: 2024 Tornado Tech <54727692+Tornado-Technology@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.NPC.HTN.Preconditions.Math;

/// <summary>
/// Checks if there is a float value for the specified <see cref="KeyFloatGreaterPrecondition.Key"/>
/// in the <see cref="NPCBlackboard"/> and the specified value is less then <see cref="KeyFloatGreaterPrecondition.Value"/>.
/// </summary>
public sealed partial class KeyFloatLessPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField(required: true), ViewVariables]
    public string Key = string.Empty;

    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public float Value;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        return blackboard.TryGetValue<float>(Key, out var value, _entManager) && value < Value;
    }
}
