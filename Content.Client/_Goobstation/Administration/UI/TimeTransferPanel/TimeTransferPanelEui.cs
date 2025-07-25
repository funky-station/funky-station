// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Client.Eui;
using Content.Shared._Goobstation.Administration;
using Content.Shared.Eui;

namespace Content.Client._Goobstation.Administration.UI.TimeTransferPanel;

public sealed class TimeTransferPanelEui : BaseEui
{
    public TimeTransferPanel TimeTransferPanel { get; }

    public TimeTransferPanelEui()
    {
        TimeTransferPanel = new TimeTransferPanel();
        TimeTransferPanel.OnTransferMessageSend += args => SendMessage(new TimeTransferEuiMessage(args.playerId, args.transferList, args.overwrite));
    }

    public override void Opened()
    {
        TimeTransferPanel.OpenCentered();
    }

    public override void Closed()
    {
        TimeTransferPanel.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not TimeTransferPanelEuiState cast)
            return;

        TimeTransferPanel.UpdateFlag(cast.HasFlag);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not TimeTransferWarningEuiMessage warning)
            return;

        TimeTransferPanel.UpdateWarning(warning.Message, warning.WarningColor);
    }
}
