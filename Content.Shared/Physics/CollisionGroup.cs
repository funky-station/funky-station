// SPDX-FileCopyrightText: 2019 L.E.D <10257081+unusualcrow@users.noreply.github.com>
// SPDX-FileCopyrightText: 2019 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2020 ancientpower <ancientpowerer@gmail.com>
// SPDX-FileCopyrightText: 2020 ancientpower <evafleck@gmail.com>
// SPDX-FileCopyrightText: 2021 Daniel Castro Razo <eldanielcr@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Ian Pike <Ianpike98@gmail.com>
// SPDX-FileCopyrightText: 2022 Jacob Tong <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Repo <47093363+Titian3@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Mish <bluscout78@yahoo.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Serialization;

namespace Content.Shared.Physics;

/// <summary>
///     Defined collision groups for the physics system.
///     Mask is what it collides with when moving. Layer is what CollisionGroup it is part of.
/// </summary>
[Flags, PublicAPI]
[FlagsFor(typeof(CollisionLayer)), FlagsFor(typeof(CollisionMask))]
public enum CollisionGroup
{
    None               = 0,
    Opaque             = 1 << 0, // 1 Blocks light, can be hit by lasers
    Impassable         = 1 << 1, // 2 Walls, objects impassable by any means
    MidImpassable      = 1 << 2, // 4 Mobs, players, crabs, etc
    HighImpassable     = 1 << 3, // 8 Things on top of tables and things that block tall/large mobs.
    LowImpassable      = 1 << 4, // 16 For things that can fit under a table or squeeze under an airlock
    GhostImpassable    = 1 << 5, // 32 Things impassible by ghosts/observers, ie blessed tiles or forcefields
    BulletImpassable   = 1 << 6, // 64 Can be hit by bullets
    InteractImpassable = 1 << 7, // 128 Blocks interaction/InRangeUnobstructed
    // Y dis door passable when all the others impassable / collision.
    DoorPassable       = 1 << 8, // 256 Allows door to close over top, Like blast doors over conveyors for disposals rooms/cargo.
    BlobImpassable     = 1 << 9, // 512 Blob Tiles Goobstation - Blob

    MapGrid = MapGridHelpers.CollisionGroup, // Map grids, like shuttles. This is the actual grid itself, not the walls or other entities connected to the grid.

    // 32 possible groups
    // Why dis exist
    AllMask = -1,

    SingularityLayer = Opaque | Impassable | MidImpassable | HighImpassable | LowImpassable | BulletImpassable | InteractImpassable | DoorPassable,

    // Humanoids, etc.
    MobMask = Impassable | HighImpassable | MidImpassable | LowImpassable | BlobImpassable, //Goobstation - Blob
    MobLayer = Opaque | BulletImpassable,
    // Mice, drones
    SmallMobMask = Impassable | LowImpassable | BlobImpassable, //Goobstation - Blob
    SmallMobLayer = Opaque | BulletImpassable,
    // Birds/other small flyers
    FlyingMobMask = Impassable | HighImpassable | BlobImpassable, //Goobstation - Blob
    FlyingMobLayer = Opaque | BulletImpassable,

    // Mechs
    LargeMobMask = Impassable | HighImpassable | MidImpassable | LowImpassable | BlobImpassable, //Goobstation - Blob
    LargeMobLayer = Opaque | HighImpassable | MidImpassable | LowImpassable | BulletImpassable,

    // Machines, computers
    MachineMask = Impassable | MidImpassable | LowImpassable | BlobImpassable, //Goobstation - Blob
    MachineLayer = Opaque | MidImpassable | LowImpassable | BulletImpassable,
    ConveyorMask = Impassable | MidImpassable | LowImpassable | DoorPassable,

    // Crates
    CrateMask = Impassable | MidImpassable | LowImpassable,

    // Tables that SmallMobs can go under
    TableMask = Impassable | MidImpassable | BlobImpassable, //Goobstation - Blob
    TableLayer = MidImpassable,

    // Tabletop machines, windoors, firelocks
    TabletopMachineMask = Impassable | HighImpassable | BlobImpassable, //Goobstation - Blob
    // Tabletop machines
    TabletopMachineLayer = Opaque | BulletImpassable,

    // Airlocks, windoors, firelocks
    GlassAirlockLayer = HighImpassable | MidImpassable | BulletImpassable | InteractImpassable,
    AirlockLayer = Opaque | GlassAirlockLayer,

    // Airlock assembly
    HumanoidBlockLayer = HighImpassable | MidImpassable,

    // Soap, spills
    SlipLayer = MidImpassable | LowImpassable,
    ItemMask = Impassable | HighImpassable,
    ThrownItem = Impassable | HighImpassable | BulletImpassable | BlobImpassable, //Goobstation - Blob
    WallLayer = Opaque | Impassable | HighImpassable | MidImpassable | LowImpassable | BulletImpassable | InteractImpassable,
    GlassLayer = Impassable | HighImpassable | MidImpassable | LowImpassable | BulletImpassable | InteractImpassable,
    HalfWallLayer = MidImpassable | LowImpassable,

    // Statue, monument, airlock, window
    FullTileMask = Impassable | HighImpassable | MidImpassable | LowImpassable | InteractImpassable,
    // FlyingMob can go past
    FullTileLayer = Opaque | HighImpassable | MidImpassable | LowImpassable | BulletImpassable | InteractImpassable,

    SubfloorMask = Impassable | LowImpassable,


    // start-goobstation: blob
    BlobMobMask = Impassable | HighImpassable | MidImpassable | LowImpassable,
    BlobMobLayer = Opaque | BulletImpassable,

    FlyingBlobMobMask = Impassable | HighImpassable,
    FlyingBlobMobLayer = Opaque | BulletImpassable,

    BlobTileLayer = Opaque | BlobImpassable | BulletImpassable
    // end-goobstation: blob
}
