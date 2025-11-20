// SPDX-FileCopyrightText: 2023 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Kot <1192090+koteq@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.SSDIndicator;

/// <summary>
///     Shows status icon when player in SSD
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class SSDIndicatorComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public bool IsSSD = true;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public ProtoId<SsdIconPrototype> Icon = "SSDIcon";

    /// <summary>
    ///     When the entity should fall asleep
    /// </summary>
    [DataField, AutoPausedField, Access(typeof(SSDIndicatorSystem))]
    public TimeSpan FallAsleepTime = TimeSpan.Zero;

    /// <summary>
    ///     Required to don't remove forced sleep from other sources
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public bool ForcedSleepAdded = false;
}
