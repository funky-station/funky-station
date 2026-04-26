// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client._Funkystation.ResourceOverview.UI;
using Content.Shared._Funkystation.ResourceOverview.BUI;
using Content.Shared._Funkystation.ResourceOverview.Components;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.ResourceOverview.BUI;

public sealed class ResourceOverviewConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private ResourceOverviewWindow? _menu;

    public ResourceOverviewConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<ResourceOverviewWindow>();
        _menu.SetEntity(Owner);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is ResourceOverviewConsoleBoundInterfaceState castState)
            _menu?.UpdateState(castState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _menu?.Dispose();
    }
}
