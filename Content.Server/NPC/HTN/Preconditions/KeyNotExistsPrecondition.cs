// SPDX-FileCopyrightText: 2024 Tornado Tech <54727692+Tornado-Technology@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.NPC.HTN.Preconditions;

/// <summary>
/// Checks if there is no value at the specified  <see cref="KeyNotExistsPrecondition.Key"/> in the <see cref="NPCBlackboard"/>.
/// Returns true if there is no value.
/// </summary>
public sealed partial class KeyNotExistsPrecondition : HTNPrecondition
{
    [DataField(required: true), ViewVariables]
    public string Key = string.Empty;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        return !blackboard.ContainsKey(Key);
    }
}
