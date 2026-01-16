// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 LankLTE <135308300+LankLTE@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Ady4ik <141335742+Ady4ik@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Zombies;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Content.Shared.Zombies;
using Robust.Shared.Timing;

namespace Content.Server.EntityEffects.Effects;

public sealed partial class CureZombieInfection : EntityEffect
{

    [DataField]
    public bool Innoculate;

    /// <summary>
    /// Immunity level (0 = no immunity, 1-5 = tiered immunity, -1 = complete immunity like Ambuzol Plus)
    /// Level 1: Cures Incubation only
    /// Level 2: Cures Incubation + Early
    /// Level 3: Cures Incubation + Early + TumorFormed
    /// Level 4: Cures all stages including Advanced
    /// Level 5: Cures all stages (same as level 4, but provides complete immunity)
    /// </summary>
    [DataField]
    public int ImmunityLevel = 0;

    /// <summary>
    /// Duration in minutes that the immunity lasts. Default is 5 minutes.
    /// </summary>
    [DataField]
    public float ImmunityDurationMinutes = 5.0f;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        if(Innoculate)
            return Loc.GetString("reagent-effect-guidebook-innoculate-zombie-infection", ("chance", Probability));

        return Loc.GetString("reagent-effect-guidebook-cure-zombie-infection", ("chance", Probability));
    }

    // Removes the Zombie Infection Components
    public override void Effect(EntityEffectBaseArgs args)
    {
        var entityManager = args.EntityManager;
        if (entityManager.HasComponent<IncurableZombieComponent>(args.TargetEntity))
            return;

        entityManager.RemoveComponent<ZombifyOnDeathComponent>(args.TargetEntity);
        entityManager.RemoveComponent<PendingZombieComponent>(args.TargetEntity);

        // Cure tumor infection based on immunity level
        if (entityManager.TryGetComponent<ZombieTumorInfectionComponent>(args.TargetEntity, out var infection))
        {
            bool shouldCure = false;

            if (ImmunityLevel > 0)
            {
                // Tiered immunity: cure based on level
                switch (ImmunityLevel)
                {
                    case 1:
                        // Level 1: Cures Incubation only
                        shouldCure = infection.Stage == ZombieTumorInfectionStage.Incubation;
                        break;
                    case 2:
                        // Level 2: Cures Incubation + Early
                        shouldCure = infection.Stage <= ZombieTumorInfectionStage.Early;
                        break;
                    case 3:
                        // Level 3: Cures Incubation + Early + TumorFormed
                        shouldCure = infection.Stage <= ZombieTumorInfectionStage.TumorFormed;
                        break;
                    case 4:
                    case 5:
                        // Level 4 and 5: Cures all stages including Advanced
                        shouldCure = true;
                        break;
                }
            }
            else if (!Innoculate)
            {
                // Original behavior: Only cure if no tumor has formed yet (Incubation or Early stage)
                // Once the tumor is formed (TumorFormed or Advanced), it requires surgery
                shouldCure = infection.Stage == ZombieTumorInfectionStage.Incubation || 
                            infection.Stage == ZombieTumorInfectionStage.Early;
            }

            if (shouldCure)
            {
                entityManager.RemoveComponent<ZombieTumorInfectionComponent>(args.TargetEntity);
            }
        }

        // Apply immunity
        if (Innoculate || ImmunityLevel == -1)
        {
            // Complete immunity (like Ambuzol Plus) - permanent immunity
            entityManager.EnsureComponent<ZombieImmuneComponent>(args.TargetEntity);
            entityManager.EnsureComponent<ZombieTumorImmuneComponent>(args.TargetEntity);
            
            // If ImmunityLevel is set (e.g., Ambuzol Plus with level 5), also apply tiered immunity without expiration
            if (ImmunityLevel > 0)
            {
                var tieredImmune = entityManager.EnsureComponent<ZombieTumorTieredImmuneComponent>(args.TargetEntity);
                tieredImmune.ImmunityLevel = ImmunityLevel;
                // Set expiration to far future for permanent immunity
                tieredImmune.ExpiresAt = TimeSpan.MaxValue;
                entityManager.Dirty(args.TargetEntity, tieredImmune);
            }
        }
        else if (ImmunityLevel > 0)
        {
            // Tiered immunity with expiration (Ambuzol I-V)
            var tieredImmune = entityManager.EnsureComponent<ZombieTumorTieredImmuneComponent>(args.TargetEntity);
            tieredImmune.ImmunityLevel = ImmunityLevel;
            // Get IGameTiming through EntitySysManager's DependencyCollection to avoid obsolete IoCManager.Resolve
            var timing = entityManager.EntitySysManager.DependencyCollection.Resolve<IGameTiming>();
            tieredImmune.ExpiresAt = timing.CurTime + TimeSpan.FromMinutes(ImmunityDurationMinutes);
            entityManager.Dirty(args.TargetEntity, tieredImmune);
        }
    }
}

