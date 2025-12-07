// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Buckle;

namespace Content.Server.Traits.Assorted;

public sealed class BuckleOnMapInitSystem : EntitySystem
{
    [Dependency] private readonly SharedBuckleSystem _buckleSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BuckleOnMapInitComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, BuckleOnMapInitComponent component, MapInitEvent args)
    {
        var buckle = Spawn(component.Prototype, Transform(uid).Coordinates);
        _buckleSystem.TryBuckle(uid, uid, buckle);
    }
}
