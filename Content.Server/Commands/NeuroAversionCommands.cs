// SPDX-License-Identifier: MIT

using Content.Server.Administration;
using Content.Server.Traits.Assorted;
using Content.Shared.Administration;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Console;
using Robust.Shared.Player;
using Robust.Server.Player;

namespace Content.Server.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class NeuroAversionDebugCommand : IConsoleCommand
{
    public string Command => "neuro_debug";
    public string Description => "Shows NeuroAversion debug info for a player or yourself";
    public string Help => "neuro_debug [player_name] - Shows seizure build, migraine timers, and mindshield status";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var playerManager = IoCManager.Resolve<IPlayerManager>();

        var target = shell.Player?.AttachedEntity;

        if (args.Length > 0)
        {
            // Find player by name
            ICommonSession? targetPlayer = null;
            foreach (var session in playerManager.Sessions)
            {
                if (session.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase))
                {
                    targetPlayer = session;
                    break;
                }
            }

            if (targetPlayer == null)
            {
                shell.WriteLine($"Player '{args[0]}' not found.");
                return;
            }
            target = targetPlayer.AttachedEntity;
        }

        if (target == null)
        {
            shell.WriteLine("No target entity found.");
            return;
        }

        if (!entityManager.TryGetComponent<NeuroAversionComponent>(target.Value, out var neuroComp))
        {
            shell.WriteLine($"Entity {entityManager.ToPrettyString(target.Value)} does not have NeuroAversionComponent.");
            return;
        }

        var hasMindshield = entityManager.HasComponent<MindShieldComponent>(target.Value);
        var isSeizing = entityManager.System<SeizureSystem>().IsSeizing(target.Value);

        // Check mob state using MobStateSystem instead of direct component access
        var mobStateSystem = entityManager.System<MobStateSystem>();
        var isDead = mobStateSystem.IsDead(target.Value);
        var isCritical = mobStateSystem.IsCritical(target.Value);
        var isAlive = mobStateSystem.IsAlive(target.Value);

        var mobStateText = isDead ? "Dead" : isCritical ? "Critical" : isAlive ? "Alive" : "Unknown";

        // Calculate seizure chance using new percentage system
        var buildFraction = neuroComp.SeizureThreshold > 0f ?
            Math.Max(0f, Math.Min(1f, neuroComp.SeizureBuild / neuroComp.SeizureThreshold)) : 0f;

        // Determine health damage fraction
        var missingHpFrac = 0f;
        if (entityManager.TryGetComponent<Content.Shared.Damage.DamageableComponent>(target.Value, out var damage))
        {
            var maxHp = damage.HealthBarThreshold?.Float() ?? 100f;
            if (maxHp > 0f)
            {
                missingHpFrac = Math.Max(0f, Math.Min(1f, (float)damage.TotalDamage / maxHp));
            }
        }

        // New timer-based system: 1% base + up to 20% from build
        var scaledBuildFraction = buildFraction * buildFraction;
        var baseChancePercent = 1f; // 1% base
        var extraChancePercent = 20f * scaledBuildFraction; // Up to 20% extra

        // Health condition multiplier
        var conditionMult = GetConditionMultiplier(neuroComp, isCritical, missingHpFrac);
        var healthModifiedBaseChance = baseChancePercent * conditionMult;
        var totalChancePercent = healthModifiedBaseChance + extraChancePercent;

        shell.WriteLine($"=== NeuroAversion Debug Info for {entityManager.ToPrettyString(target.Value)} ===");
        shell.WriteLine($"Has Mindshield: {(hasMindshield ? "YES" : "NO")}");
        shell.WriteLine($"Mindshield Status Cached: {neuroComp.IsMindShielded}");
        shell.WriteLine($"Mob State: {mobStateText} (Dead: {isDead}, Critical: {isCritical})");
        shell.WriteLine($"Currently Seizing: {(isSeizing ? "YES" : "NO")}");
        shell.WriteLine($"Seizure Build: {neuroComp.SeizureBuild:F3} / {neuroComp.SeizureThreshold:F1} ({buildFraction * 100:F1}%)");
        shell.WriteLine($"Seizure Chance (every 3s): Base={healthModifiedBaseChance:F1}%, Extra={extraChancePercent:F1}%, Total={totalChancePercent:F1}%");
        shell.WriteLine($"Next Migraine In: {neuroComp.NextMigraineTime:F1}s");
        shell.WriteLine($"Next Seizure Roll In: {neuroComp.NextSeizureRollTime:F1}s");
        shell.WriteLine($"Health Damage: {missingHpFrac * 100:F1}%");

        // Show blocking reasons
        if (!neuroComp.IsMindShielded)
            shell.WriteLine($"🚫 SEIZURES BLOCKED: No mindshield!");
        else if (isDead)
            shell.WriteLine($"🚫 SEIZURES BLOCKED: Entity is dead!");
        else if (isCritical)
            shell.WriteLine($"🚫 SEIZURES BLOCKED: Entity is in critical condition!");
        else if (isSeizing)
            shell.WriteLine($"🚫 SEIZURES BLOCKED: Already having a seizure!");
        else
            shell.WriteLine($"✅ SEIZURES ACTIVE: Rolling for seizure each frame!");
    }

    private static float GetConditionMultiplier(NeuroAversionComponent comp, bool isCritical, float missingHpFrac)
    {
        if (isCritical)
            return comp.ConditionCriticalMultiplier;
        if (missingHpFrac >= 2f / 3f)
            return comp.ConditionBadMultiplier;
        if (missingHpFrac >= 1f / 3f)
            return comp.ConditionOkayMultiplier;
        return comp.ConditionGoodMultiplier;
    }
}

[AdminCommand(AdminFlags.Debug)]
public sealed class NeuroAversionAddBuildCommand : IConsoleCommand
{
    public string Command => "neuro_addbuild";
    public string Description => "Adds seizure build to a player";
    public string Help => "neuro_addbuild <amount> [player_name] - Adds build to target (can be negative)";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 1)
        {
            shell.WriteLine("Usage: neuro_addbuild <amount> [player_name]");
            return;
        }

        if (!float.TryParse(args[0], out var amount))
        {
            shell.WriteLine("Invalid amount specified.");
            return;
        }

        var entityManager = IoCManager.Resolve<IEntityManager>();
        var playerManager = IoCManager.Resolve<IPlayerManager>();

        var target = shell.Player?.AttachedEntity;

        if (args.Length > 1)
        {
            // Find player by name
            ICommonSession? targetPlayer = null;
            foreach (var session in playerManager.Sessions)
            {
                if (session.Name.Equals(args[1], StringComparison.OrdinalIgnoreCase))
                {
                    targetPlayer = session;
                    break;
                }
            }

            if (targetPlayer == null)
            {
                shell.WriteLine($"Player '{args[1]}' not found.");
                return;
            }
            target = targetPlayer.AttachedEntity;
        }

        if (target == null)
        {
            shell.WriteLine("No target entity found.");
            return;
        }

        var neuroSystem = entityManager.System<NeuroAversionSystem>();
        neuroSystem.ModifySeizureBuild(target.Value, amount);

        shell.WriteLine($"Added {amount:F2} seizure build to {entityManager.ToPrettyString(target.Value)}");
    }
}

[AdminCommand(AdminFlags.Debug)]
public sealed class NeuroAversionTriggerSeizureCommand : IConsoleCommand
{
    public string Command => "neuro_seizure";
    public string Description => "Triggers a seizure on a player";
    public string Help => "neuro_seizure [player_name] - Immediately triggers a seizure";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var playerManager = IoCManager.Resolve<IPlayerManager>();

        var target = shell.Player?.AttachedEntity;

        if (args.Length > 0)
        {
            // Find player by name
            ICommonSession? targetPlayer = null;
            foreach (var session in playerManager.Sessions)
            {
                if (session.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase))
                {
                    targetPlayer = session;
                    break;
                }
            }

            if (targetPlayer == null)
            {
                shell.WriteLine($"Player '{args[0]}' not found.");
                return;
            }
            target = targetPlayer.AttachedEntity;
        }

        if (target == null)
        {
            shell.WriteLine("No target entity found.");
            return;
        }

        var neuroSystem = entityManager.System<NeuroAversionSystem>();
        var seizureSystem = entityManager.System<SeizureSystem>();

        if (seizureSystem.IsSeizing(target.Value))
        {
            shell.WriteLine($"{entityManager.ToPrettyString(target.Value)} is already having a seizure!");
            return;
        }

        if (neuroSystem.TryTriggerSeizure(target.Value))
        {
            shell.WriteLine($"Triggered seizure on {entityManager.ToPrettyString(target.Value)}");
        }
        else
        {
            shell.WriteLine($"Failed to trigger seizure on {entityManager.ToPrettyString(target.Value)} (no NeuroAversionComponent?)");
        }
    }
}
