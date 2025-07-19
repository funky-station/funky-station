// SPDX-FileCopyrightText: 2025 SaffronFennec <firefoxwolf2020@protonmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Traits;
using Robust.Shared.Timing;

namespace Content.Server.Traits;

public sealed class LiquorLifelineSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;

    private float _accumulator;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LiquorLifelineComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, LiquorLifelineComponent component, ComponentStartup args)
    {
        if (!TryComp<BodyComponent>(uid, out var body))
            return;

        var root = _bodySystem.GetRootPartOrNull(uid, body);
        if (root == null)
            return;

        // Find all organs in the torso.
        foreach (var organ in _bodySystem.GetPartOrgans(root.Value.Entity, root.Value.BodyPart))
        {
            // If we find a liver, remove it and replace it with a dwarf liver.
            if (organ.Component.SlotId == "liver")
            {
                _bodySystem.RemoveOrgan(organ.Id);
                QueueDel(organ.Id);
                var liver = Spawn("OrganDwarfLiver", Transform(uid).Coordinates);
                _bodySystem.InsertOrgan(root.Value.Entity, liver, "liver");
                break;
            }
        }
    }
}
