// SPDX-FileCopyrightText: 2025 otokonoko-dev <248204705+otokonoko-dev@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Medical;
using Content.Shared.Verbs;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Utility;
using Robust.Shared.GameObjects;

namespace Content.Server.Medical;

public sealed class BedWheelsSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BedWheelsComponent, GetVerbsEvent<InteractionVerb>>(OnGetVerbs);
    }

    private void OnGetVerbs(
        EntityUid uid,
        BedWheelsComponent comp,
        GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var verb = new InteractionVerb
        {
            Text = comp.Locked ? "Unlock wheels" : "Lock wheels",
            Icon = new SpriteSpecifier.Texture(new (comp.Locked ? "/Textures/Interface/VerbIcons/unlock.svg.192dpi.png" : "/Textures/Interface/VerbIcons/lock.svg.192dpi.png")),
            Act = () =>
            {
                comp.Locked = !comp.Locked;

                var xform = Transform(uid);

                if (comp.Locked)
                {
                    if (TryComp<PullableComponent>(uid, out var pullable))
                    {
                        _pulling.TryStopPull(uid, pullable, args.User);
                    }

                    _transform.AnchorEntity(uid, xform);
                }
                else
                {
                    _transform.Unanchor(uid, xform);
                }
            }
        };

        args.Verbs.Add(verb);
    }
}
