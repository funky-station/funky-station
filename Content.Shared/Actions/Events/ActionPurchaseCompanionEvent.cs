// SPDX-FileCopyrightText: 2025 AI Assistant
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.Actions.Events;

/// <summary>
/// Event raised when a store purchase should grant companion actions.
/// This allows store listings to automatically grant additional actions
/// when a primary action is purchased.
/// </summary>
[Serializable, NetSerializable, DataDefinition]
public sealed partial class ActionPurchaseCompanionEvent : EntityEventArgs
{
    public NetEntity Buyer { get; set; }
    [DataField("companionActions")]
    public List<string> CompanionActions { get; set; } = new();

    public ActionPurchaseCompanionEvent()
    {
    }

    public ActionPurchaseCompanionEvent(NetEntity buyer, List<string> companionActions)
    {
        Buyer = buyer;
        CompanionActions = companionActions;
    }
}
