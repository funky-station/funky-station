// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Shared.Random.Rules;

/// <summary>
/// Rules-based item selection. Can be used for any sort of conditional selection
/// Every single condition needs to be true for this to be selected.
/// e.g. "choose maintenance audio if 90% of tiles nearby are maintenance tiles"
/// </summary>
[Prototype]
public sealed partial class RulesPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = string.Empty;

    [DataField("rules", required: true)]
    public List<RulesRule> Rules = new();
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class RulesRule
{
    [DataField]
    public bool Inverted;
    public abstract bool Check(EntityManager entManager, EntityUid uid);
}

public sealed class RulesSystem : EntitySystem
{
    public bool IsTrue(EntityUid uid, RulesPrototype rules)
    {
        foreach (var rule in rules.Rules)
        {
            if (!rule.Check(EntityManager, uid))
                return false;
        }

        return true;
    }
}
