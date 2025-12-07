// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Cloning;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
///     Gamerule component for spawning a paradox clone antagonist.
/// </summary>
[RegisterComponent]
public sealed partial class ParadoxCloneRuleComponent : Component
{
    /// <summary>
    ///     Cloning settings to be used.
    /// </summary>
    [DataField]
    public ProtoId<CloningSettingsPrototype> Settings = "Antag";

    /// <summary>
    ///     Visual effect spawned when gibbing at round end.
    /// </summary>
    [DataField]
    public EntProtoId GibProto = "MobParadoxTimed";

    /// <summary>
    ///     Entity of the original player.
    ///     Gets randomly chosen from all alive players if not specified.
    /// </summary>
    [DataField]
    public EntityUid? OriginalBody;

    /// <summary>
    ///     Mind entity of the original player.
    ///     Gets assigned when cloning.
    /// </summary>
    [DataField]
    public EntityUid? OriginalMind;

    /// <summary>
    ///     Whitelist for Objectives to be copied to the clone.
    /// </summary>
    [DataField]
    public EntityWhitelist? ObjectiveWhitelist;

    /// <summary>
    ///     Blacklist for Objectives to be copied to the clone.
    /// </summary>
    [DataField]
    public EntityWhitelist? ObjectiveBlacklist;
}
