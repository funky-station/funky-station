// SPDX-FileCopyrightText: 2025 Falcon <falcon@zigtag.dev>
// SPDX-FileCopyrightText: 2025 pre-commit-ci[bot] <66853113+pre-commit-ci[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._DV.Silicon.IPC;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;

namespace Content.Client._DV.Silicon.IPC;

public sealed class SnoutHelmetSystem : EntitySystem
{
    private const MarkingCategories MarkingToQuery = MarkingCategories.Snout;
    private const int MaximumMarkingCount = 0;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SnoutHelmetComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(EntityUid uid, SnoutHelmetComponent component, ComponentStartup args)
    {
        if (TryComp(uid, out HumanoidAppearanceComponent? humanoidAppearanceComponent) &&
            humanoidAppearanceComponent.ClientOldMarkings.Markings.TryGetValue(MarkingToQuery, out var markings) &&
            markings.Count > MaximumMarkingCount)
        {
            component.EnableAlternateHelmet = true;
        }
    }
}
