// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Server.MalfAI;

/// <summary>
/// Malf AI purchase events: raised by the store when specific upgrades are bought.
/// </summary>


[Serializable, DataDefinition]
public sealed partial class MalfAiSyndicateKeysUnlockedEvent : EntityEventArgs
{
}

[Serializable, DataDefinition]
public sealed partial class MalfAiCameraUpgradeUnlockedEvent : EntityEventArgs
{
}

[Serializable, DataDefinition]
public sealed partial class MalfAiCameraMicrophonesUnlockedEvent : EntityEventArgs
{
}
