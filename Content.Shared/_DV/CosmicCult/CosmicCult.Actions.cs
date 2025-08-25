// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.CosmicCult;

[RegisterComponent, NetworkedComponent]
public sealed partial class CosmicCultActionComponent : Component;
public sealed partial class EventCosmicSiphon : EntityTargetActionEvent;
public sealed partial class EventCosmicBlank : EntityTargetActionEvent;
public sealed partial class EventCosmicPlaceMonument : InstantActionEvent; //given to the cult leader on roundstart
public sealed partial class EventCosmicMoveMonument : InstantActionEvent; //given the the cult leader on hitting tier 2, taken away on hitting tier 3
public sealed partial class EventCosmicReturn : InstantActionEvent;
public sealed partial class EventCosmicLapse : EntityTargetActionEvent;
public sealed partial class EventCosmicGlare : InstantActionEvent;
public sealed partial class EventCosmicIngress : EntityTargetActionEvent;
public sealed partial class EventCosmicImposition : InstantActionEvent;
public sealed partial class EventCosmicNova : WorldTargetActionEvent;
public sealed partial class EventCosmicFragmentation : EntityTargetActionEvent;
