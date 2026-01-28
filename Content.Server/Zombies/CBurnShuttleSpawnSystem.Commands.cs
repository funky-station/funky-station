// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.Administration;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Console;

namespace Content.Server.Zombies;

public sealed partial class CBurnShuttleSpawnSystem
{
    [Dependency] private readonly IConsoleHost _consoleHost = default!;

    private void InitializeCommands()
    {
        _consoleHost.RegisterCommand("testspawncburn", "Test command to manually spawn CBURN shuttles. Usage: testspawncburn [count] [force]", "testspawncburn",
            TestSpawnCBurnCommand);
    }

    [AdminCommand(AdminFlags.Fun)]
    private void TestSpawnCBurnCommand(IConsoleShell shell, string argStr, string[] args)
    {
        // Parse arguments: ghostRoleCount (optional integer) and force flag (optional)
        int? ghostRoleCount = null;
        bool force = false;

        foreach (var arg in args)
        {
            if (arg.ToLower() == "force")
            {
                force = true;
            }
            else if (int.TryParse(arg, out var count))
            {
                if (count < 1)
                {
                    shell.WriteError($"Ghost role count must be at least 1, got {count}");
                    return;
                }
                ghostRoleCount = count;
            }
            else
            {
                shell.WriteError($"Invalid argument: {arg}. Expected a number or 'force'");
                return;
            }
        }

        EntityUid ruleEntity;
        CBurnShuttleSpawnComponent spawnComp;

        if (force)
        {
            // Force mode: create a temporary entity with the spawn component
            ruleEntity = Spawn(null);
            spawnComp = EnsureComp<CBurnShuttleSpawnComponent>(ruleEntity);
            shell.WriteLine($"Force mode: Ignoring game rule. Spawning CBURN shuttles with {(ghostRoleCount.HasValue ? $"{ghostRoleCount.Value} ghost role(s)" : "auto-calculated ghost roles")}...");
        }
        else
        {
            // Normal mode: find the zombie rule entity
            EntityUid? zombieRuleUid = null;
            
            // First try to find an active rule
            var activeQuery = EntityQueryEnumerator<ZombieRuleComponent, ActiveGameRuleComponent>();
            if (activeQuery.MoveNext(out var activeUid, out _, out _))
            {
                zombieRuleUid = activeUid;
            }
            else
            {
                // If no active rule, find any zombie rule for testing
                var query = EntityQueryEnumerator<ZombieRuleComponent, GameRuleComponent>();
                if (query.MoveNext(out var uid, out _, out _))
                {
                    zombieRuleUid = uid;
                }
            }

            if (zombieRuleUid == null)
            {
                shell.WriteError("No active zombie rule found. Start a zombie round first, or use 'force' to spawn without a rule.");
                return;
            }

            ruleEntity = zombieRuleUid.Value;
            spawnComp = EnsureComp<CBurnShuttleSpawnComponent>(ruleEntity);

            // Allow resetting and re-spawning for testing
            if (spawnComp.HasSpawned)
            {
                shell.WriteLine("CBURN shuttles have already been spawned. Use 'testspawncburn [count] force' to spawn again.");
                return;
            }

            shell.WriteLine($"Spawning CBURN shuttles with {(ghostRoleCount.HasValue ? $"{ghostRoleCount.Value} ghost role(s)" : "auto-calculated ghost roles")}...");
        }

        SpawnCBurnShuttles(spawnComp, shuttleCount: null, maxGhostRoles: ghostRoleCount);
        spawnComp.HasSpawned = true;

        // Clean up temporary entity if force mode
        if (force)
        {
            QueueDel(ruleEntity);
        }
        
        shell.WriteLine($"CBURN shuttle spawn completed. Check logs for details.");
    }
}
