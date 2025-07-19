// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Xenoarchaeology.Artifact.XAT.Components;

/// <summary>
/// This is used for a xenoarch trigger that activates when a reaction occurs on the artifact.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(XATReactiveSystem)), AutoGenerateComponentState]
public sealed partial class XATReactiveComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ReactionMethod> ReactionMethods = new() { ReactionMethod.Touch };

    /// <summary>
    /// Reagents that are required in quantity <see cref="MinQuantity"/> to activate trigger.
    /// If any of them are present in required amount - activation will be triggered.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<ReagentPrototype>> Reagents = new();

    /// <summary>
    /// ReagentGroups that are required in quantity <see cref="MinQuantity"/> to activate trigger.
    /// If any of them are present in required amount - activation will be triggered.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<ReactiveGroupPrototype>> ReactiveGroups = new();

    /// <summary>
    /// Min amount of reagent to trigger.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 MinQuantity = 5f;
}
