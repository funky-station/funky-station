// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;

namespace Content.Shared._DV.Surgery;

/// <summary>
///     Component that allows an entity to be cross contamined from being used in surgery
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SurgeryCleanSystem))]
[AutoGenerateComponentState]
public sealed partial class SurgeryCrossContaminationComponent : Component
{
    /// <summary>
    ///     Patient DNAs that are present on this dirtied tool
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<string> DNAs = new();
}
