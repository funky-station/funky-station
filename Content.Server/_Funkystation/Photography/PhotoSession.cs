﻿using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Server._Funkystation.Photography;

public sealed class PhotoSession
{
    /// <summary>
    ///     The center position of this session.
    /// </summary>
    public readonly MapCoordinates Position;

    /// <summary>
    ///     The set of players currently viewing this photograph.
    /// </summary>
    public readonly Dictionary<ICommonSession, PhotoSessionPlayerData> Players = new();

    /// <summary>
    ///     All entities bound to this session. If you create an entity for this session, you have to add it here.
    /// </summary>
    public readonly HashSet<EntityUid> Entities = new();

    public PhotoSession(MapId tabletopMap, Vector2 position)
    {
        Position = new MapCoordinates(position, tabletopMap);
    }
}
