// SPDX-FileCopyrightText: 2025 ATDoop <bug@bug.bug>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Client._Impstation.Thaven;
using Content.Shared._Impstation.Thaven;
using Content.Shared._Impstation.Thaven.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Impstation.Thaven;

[UsedImplicitly]
public sealed class ThavenMoodsBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private ThavenMoodsMenu? _menu;
    private EntityUid _owner;
    private List<ThavenMood>? _moods;
    private List<ThavenMood>? _sharedMoods;

    public ThavenMoodsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _owner = owner;
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<ThavenMoodsMenu>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not ThavenMoodsBuiState msg)
            return;

        _moods = msg.Moods;
        _sharedMoods = msg.SharedMoods;
        _menu?.Update(_owner, msg);
    }
}
