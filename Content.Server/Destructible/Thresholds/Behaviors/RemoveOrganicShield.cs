// SPDX-FileCopyrightText: 2025 Drywink <43855731+Drywink@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Changeling;

namespace Content.Server.Destructible.Thresholds.Behaviors
{
    [DataDefinition]
    public sealed partial class RemoveOrganicShield : IThresholdBehavior
    {
        public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
        {
            var entManager = IoCManager.Resolve<IEntityManager>();
            //Logger.Debug($"[RemoveOrganicShield] Executing on entity {owner}");

            // Climb the parent chain to find the root entity with the ChangelingComponent
            var current = owner;
            while (entManager.TryGetComponent(current, out TransformComponent? xform) && xform.ParentUid.IsValid())
            {
                current = xform.ParentUid;

                if (entManager.HasComponent<ChangelingComponent>(current))
                {
                    if (entManager.TryGetComponent(current, out ChangelingComponent? changelingComponent))
                    {
                        // Logger.Debug($"[RemoveOrganicShield] Found ChangelingComponent on {current}, removing shield.");
                        changelingComponent.Equipment.Remove(changelingComponent.ShieldPrototype);
                        return;
                    }
                }
            }
            Logger.Warning($"[RemoveOrganicShield] Could not find owning entity with ChangelingComponent for {owner}");
        }
    }
}
