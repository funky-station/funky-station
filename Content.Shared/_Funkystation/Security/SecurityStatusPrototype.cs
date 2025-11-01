// SPDX-FileCopyrightText: 2025 Ilya Mikheev <me@ilyamikcoder.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;
using Content.Shared.StatusIcon;

namespace Content.Shared._Funkystation.Security;

/// <summary>
/// SecurityStatuses to be applied from a criminal records console
/// </summary>
[Prototype]
public sealed partial class SecurityStatusPrototype : IPrototype, IComparable
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Name of the status to be shown in interface.
    /// </summary>
    [DataField(required: true)]
    public string Name = string.Empty;

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedName => Loc.GetString(Name);

    /// <summary>
    /// Status icon to be shown on security HUDs.
    /// </summary>
    [DataField]
    public ProtoId<SecurityIconPrototype>? SecurityIcon;

    /// <summary>
    /// Require an officer to provide a reason for setting the status?
    /// </summary>
    [DataField]
    public bool RequiresReason = false;

    /// <summary>
    /// Statuses with higher priority will be listed higher on criminal record consoles.
    /// </summary>
    [DataField]
    public int Priority = -999;

    /// <summary>
    /// Label to be displayed in the wanted list cartridge. Should include color markup.
    /// </summary>
    [DataField]
    public string CartridgeLabel = string.Empty;

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedCartridgeLabel => Loc.GetString(CartridgeLabel);

    /// <summary>
    /// Radio message to be sent to the security channel once the status is set.
    /// </summary>
    [DataField]
    public string RadioMessage = string.Empty;

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedRadioMessage => Loc.GetString(RadioMessage);

    /// <summary>
    /// Radio message to be sent to the security channel once the status is removed.
    /// </summary>
    [DataField]
    public string RemovalRadioMessage = string.Empty;

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedRemovalRadioMessage => Loc.GetString(RemovalRadioMessage);

    public int CompareTo(object? obj)
    {
        if (obj == null) return 1;

        if (obj is SecurityStatusPrototype otherStatus)
            return Name.CompareTo(otherStatus.Name);
        else
            return 1;
    }

    public bool IsNone()
    {
        return ID == "SecurityStatusNone";
    }
}
