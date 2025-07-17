// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Analyzers;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.Autoclave;

/// <summary>
///     A component that will cause powered locker entities to clean their contents periodically
/// </summary>
[RegisterComponent, AutoGenerateComponentPause, Access(typeof(AutoclaveSystem))]
public sealed partial class AutoclaveComponent : Component
{
    /// <summary>
    ///     The next time that items inside the autoclave will be cleaned
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate;

    /// <summary>
    ///     How often to clean contained items
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);
}
