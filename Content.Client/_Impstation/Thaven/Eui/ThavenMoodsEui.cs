// SPDX-FileCopyrightText: 2025 ATDoop <bug@bug.bug>
// SPDX-FileCopyrightText: 2025 Dark <darkwindleaf@hotmail.co.uk>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Client.Eui;
using Content.Shared.Eui;
using Content.Shared._Impstation.Thaven;

namespace Content.Client._Impstation.Thaven.Eui;

public sealed class ThavenMoodsEui : BaseEui
{
    private ThavenMoodUi _thavenMoodUi;
    private NetEntity _target;

    public ThavenMoodsEui()
    {
        _thavenMoodUi = new ThavenMoodUi();
        _thavenMoodUi.OnSave += SaveMoods;
    }

    private void SaveMoods()
    {
        var newMoods = _thavenMoodUi.GetMoods();
        SendMessage(new ThavenMoodsSaveMessage(newMoods, _target));
        _thavenMoodUi.SetMoods(newMoods);
    }

    public override void Opened()
    {
        _thavenMoodUi.OpenCentered();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not ThavenMoodsEuiState s)
            return;

        _target = s.Target;
        _thavenMoodUi.SetMoods(s.Moods);
    }
}
