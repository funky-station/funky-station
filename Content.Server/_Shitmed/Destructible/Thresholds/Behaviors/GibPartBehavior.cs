// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using JetBrains.Annotations;

// Leaving this one in the default namespace because I am afraid to test it 
// in the Shitmed namespace lmao.
namespace Content.Server.Destructible.Thresholds.Behaviors;

[UsedImplicitly]
[DataDefinition]
public sealed partial class GibPartBehavior : IThresholdBehavior
{
    public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
    {
        if (!system.EntityManager.TryGetComponent(owner, out BodyPartComponent? part))
            return;

        system.BodySystem.GibPart(owner, part);
    }
}

