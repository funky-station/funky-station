// SPDX-FileCopyrightText: 2025 rotty <juaelwe@outlook.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Power.NodeGroups;
using Content.Server.Power.Components;

namespace Content.Server._NF.Power.Components;

/// <summary>
///     Marks an entity as capable of using both APC and battery power.
/// </summary>
[RegisterComponent]
public sealed partial class MixedPowerReceiverComponent : Component
{

}

