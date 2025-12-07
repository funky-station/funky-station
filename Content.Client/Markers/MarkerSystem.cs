// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Vince <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ike709 <ike709@github.com>
// SPDX-FileCopyrightText: 2022 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;

namespace Content.Client.Markers;

public sealed class MarkerSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private bool _markersVisible;

    public bool MarkersVisible
    {
        get => _markersVisible;
        set
        {
            _markersVisible = value;
            UpdateMarkers();
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MarkerComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, MarkerComponent marker, ComponentStartup args)
    {
        UpdateVisibility(uid);
    }

    private void UpdateVisibility(EntityUid uid)
    {
        if (EntityManager.TryGetComponent(uid, out SpriteComponent? sprite))
        {
            _sprite.SetVisible((uid, sprite), MarkersVisible);
        }
    }

    private void UpdateMarkers()
    {
        var query = AllEntityQuery<MarkerComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            UpdateVisibility(uid);
        }
    }
}
