// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Random.Rules;

/// <summary>
/// Always returns true. Used for fallbacks.
/// </summary>
public sealed partial class AlwaysTrueRule : RulesRule
{
    public override bool Check(EntityManager entManager, EntityUid uid)
    {
        return !Inverted;
    }
}
