// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.Graphics;

namespace Content.Client.Shuttles.Systems;

public sealed partial class ShuttleSystem
{
    [Dependency] private readonly IOverlayManager _overlays = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeEmergency();
        _overlays.AddOverlay(new FtlArrivalOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlays.RemoveOverlay<FtlArrivalOverlay>();
    }
}
