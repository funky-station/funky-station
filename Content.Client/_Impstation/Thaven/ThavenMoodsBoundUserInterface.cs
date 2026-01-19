// SPDX-FileCopyrightText: 2025 ATDoop <bug@bug.bug>
// SPDX-FileCopyrightText: 2025 Dark <darkwindleaf@hotmail.co.uk>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._Impstation.Thaven;
using Content.Shared._Impstation.Thaven.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Impstation.Thaven;

[UsedImplicitly]
public sealed class ThavenMoodsBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entMan = default!;

    [ViewVariables]
    private ThavenMoodsMenu? _menu;

    public ThavenMoodsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
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

        if (!_entMan.TryGetComponent<ThavenMoodsComponent>(Owner, out var comp))
            return;

        _menu?.Update(comp, msg);
    }
}
