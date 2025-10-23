// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using System.Text.Json.Serialization;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Database;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Toolshed.TypeParsers;

namespace Content.Shared.EntityEffects;

/// <summary>
/// A basic instantaneous effect which can be applied to an entity via events.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class EntityEffect
{
    public abstract void RaiseEvent(EntityUid target, IEntityEffectRaiser raiser, float scale, EntityUid? user);

    [DataField]
    public EntityCondition[]? Conditions;

    /// <summary>
    /// If our scale is less than this value, the effect fails.
    /// </summary>
    [DataField]
    public virtual float MinScale { get; private set; }

    /// <summary>
    /// If true, then it allows the scale multiplier to go above 1.
    /// </summary>
    [DataField]
    public virtual bool Scaling { get; private set; }

    // TODO: This should be an entity condition but guidebook relies on it heavily for formatting...
    /// <summary>
    /// Probability of the effect occuring.
    /// </summary>
    [DataField]
    public float Probability = 1.0f;

    public virtual string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => null;

    /// <summary>
    /// If this effect is logged, how important is the log?
    /// </summary>
    [ViewVariables]
    public virtual LogImpact? Impact => null;

    [ViewVariables]
    public virtual LogType LogType => LogType.EntityEffect;
}

/// <summary>
/// Used to store an <see cref="EntityEffect"/> so it can be raised without losing the type of the condition.
/// </summary>
/// <typeparam name="T">The Condition wer are raising.</typeparam>
public abstract partial class EntityEffectBase<T> : EntityEffect where T : EntityEffectBase<T>
{
    public override void RaiseEvent(EntityUid target, IEntityEffectRaiser raiser, float scale, EntityUid? user)
    {
        if (this is not T type)
            return;

        raiser.RaiseEffectEvent(target, type, scale, user);
    }
}
