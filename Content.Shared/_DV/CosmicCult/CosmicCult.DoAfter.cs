// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.CosmicCult;

[Serializable, NetSerializable]
public sealed partial class EventCosmicSiphonDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class EventCosmicBlankDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class EventAbsorbRiftDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class EventPurgeRiftDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class StartFinaleDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class CancelFinaleDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class EventCosmicFragmentationDoAfter : SimpleDoAfterEvent;
