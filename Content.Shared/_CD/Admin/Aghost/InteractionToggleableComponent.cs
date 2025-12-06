// SPDX-FileCopyrightText: 2025 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CD.Admin.Aghost;

/// <summary>
/// Component that determines if aghosts should be able to interact with things
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InteractionToggleableComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> ToggleAlertProtoId = "ToggleInteraction";

    [DataField, AutoNetworkedField]
    public bool BlockInteraction;
}

/// <summary>
/// Event raised to toggle aghost interaction
/// </summary>
public sealed partial class ToggleInteractionEvent : BaseAlertEvent;
