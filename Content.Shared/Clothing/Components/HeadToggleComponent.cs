// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Clothing.EntitySystems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Shared.Clothing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(HeadToggleSystem))]
public sealed partial class HeadToggleComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ToggleAction = "ActionToggleHead";

    /// <summary>
    /// The action entity for toggling this item
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;

    [DataField, AutoNetworkedField]
    public bool IsToggled;

    /// <summary>
    /// Equipped prefix to use after the helmet/visor is toggled
    /// For a welding helmet, this is usually up
    /// </summary>
    [DataField, AutoNetworkedField]
    public string EquippedPrefix = "up";

    /// <summary>
    /// When <see langword="true"/> will function normally, otherwise will not react to events
    /// </summary>
    [DataField("enabled"), AutoNetworkedField]
    public bool IsEnabled = true;

    /// <summary>
    /// If true, the logic is inverted. Starts toggled up (inactive), and toggling activates components
    /// </summary>
    [DataField("invertLogic"), AutoNetworkedField]
    public bool InvertLogic;

    /// <summary>
    /// Sound to play when the visor is activated
    /// </summary>
    [DataField("soundToggleOn"), AutoNetworkedField]
    public SoundSpecifier? SoundToggleOn;

    /// <summary>
    /// Sound to play when the visor is deactivated
    /// </summary>
    [DataField("soundToggleOff"), AutoNetworkedField]
    public SoundSpecifier? SoundToggleOff;

    /// <summary>
    /// Cooldown time for the toggle action
    /// </summary>
    [DataField("cooldown"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan? Cooldown;

}
