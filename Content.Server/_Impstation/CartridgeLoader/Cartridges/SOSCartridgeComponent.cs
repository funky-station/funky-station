// SPDX-FileCopyrightText: 2025 Josh Hilsberg <thejoulesberg@gmail.com>
// SPDX-FileCopyrightText: 2025 JoulesBerg <104539820+JoulesBerg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 PurpleTranStar <purpletranstars@gmail.com>
// SPDX-FileCopyrightText: 2025 PurpleTranStar <tehevilduckiscoming@gmail.com>
// SPDX-FileCopyrightText: 2025 mqole <113324899+mqole@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 rosieposie <52761126+rosieposieeee@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server._Impstation.CartridgeLoader.Cartridges;

[RegisterComponent]
public sealed partial class SOSCartridgeComponent : Component
{
    //Path to the id container
    public const string PDAIdContainer = "PDA-id";

    [DataField]
    //Name to use if no id is found
    public string DefaultName = "sos-caller-defaultname";

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedDefaultName => Loc.GetString(DefaultName);

    [DataField]
    //Notification message
    public string HelpMessage = "sos-message";

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedHelpMessage => Loc.GetString(HelpMessage);

    [DataField]
    //Channel to notify
    public ProtoId<RadioChannelPrototype> HelpChannel = "Emergency";

    [DataField]
    //Timeout between calls
    public const float TimeOut = 90;

    [DataField]
    //Countdown until next call is allowed
    public float Timer = 0;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool CanCall => Timer <= 0;
}
