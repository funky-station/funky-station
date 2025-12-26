// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Station.Components;
using Content.Shared.StationEvents.Events;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.StationEvents;

public sealed partial class IonStormVisualsSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly Dictionary<MapId, TimeSpan> _ionStormEnds = [];
    private IonStormOverlay _overlay = default!;

    private static readonly TimeSpan IonStormLength = TimeSpan.FromSeconds(2.5);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);

        SubscribeNetworkEvent<IonStormedEvent>(OnIonStorm);

        _overlay = new();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        RemQueue<MapId> toRemove = new();

        foreach (var (station, endTime) in _ionStormEnds)
        {
            var stillActive = endTime > _timing.CurTime;
            if (!stillActive && !_overlay.IsVisible(station))
                toRemove.Add(station);
        }

        foreach (var station in toRemove)
        {
            _ionStormEnds.Remove(station);
            _overlay.RemoveMap(station);
        }
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        foreach (var (station, endTime) in _ionStormEnds)
        {
            var stillActive = endTime > _timing.CurTime;
            _overlay.UpdateFade(station, frameTime, stillActive);
        }
    }

    private void OnPlayerAttached(LocalPlayerAttachedEvent args)
    {
        _overlayManager.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(LocalPlayerDetachedEvent args)
    {
        _overlayManager.RemoveOverlay(_overlay);
    }

    private void OnIonStorm(IonStormedEvent args)
    {
        var station = GetEntity(args.Station);

        // station data isnt in shared so you get this fuck you
        var query = EntityQueryEnumerator<StationMemberComponent>();
        while (query.MoveNext(out var grid, out var stationMember))
        {
            if (stationMember.Station != station)
                continue;

            var map = Transform(grid).MapID;

            _overlay.AddMap(map);
            _ionStormEnds[map] = _timing.CurTime + IonStormLength;
        }
    }
}
