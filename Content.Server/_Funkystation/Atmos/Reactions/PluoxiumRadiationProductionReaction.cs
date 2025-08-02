// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.Nodes;
using Content.Server.NodeContainer.NodeGroups;
using Content.Server.Radiation.Components;
using Content.Server.Radiation.Events;
using Content.Shared.Radiation.Systems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using Content.Shared.Radiation.Components;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Server.GameObjects; 
using Robust.Shared.Timing; 
using Robust.Shared.Serialization; 
using Robust.Shared.IoC;
using Content.Shared.Stacks;
using Content.Server.Radiation.Systems;
using System.Linq;
using System.Numerics;
using Robust.Shared.Map.Components;
using Robust.Shared.GameObjects;
using Content.Server.Atmos.Components;


namespace Content.Server.Atmos.Reactions;

/// <summary>
///     Assmos - /tg/ gases
///     Consumes a tiny amount of tritium to convert CO2 and oxygen to pluoxium.
/// </summary>
[UsedImplicitly]
public sealed partial class PluoxiumRadiationProductionReaction : IGasReactionEffect
{
    private IEntityManager entityManager => IoCManager.Resolve<IEntityManager>();
    private IGameTiming gameTiming => IoCManager.Resolve<IGameTiming>();
    private IEntitySystemManager systemManager => IoCManager.Resolve<IEntitySystemManager>();
    private RadiationSystem radiationSystem => systemManager.GetEntitySystem<RadiationSystem>();
    private const float RadiationThreshold = 0.01f;
    private static readonly TimeSpan TimerDuration = TimeSpan.FromSeconds(5);

    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        if (entityManager == null || gameTiming == null)
            return ReactionResult.NoReaction;

        float initO2 = mixture.GetMoles(Gas.Oxygen);
        float initCO2 = mixture.GetMoles(Gas.CarbonDioxide);

        float radiationLevel = 0f;

        if (holder is null)
        {
            return ReactionResult.NoReaction;
        }
        else if (holder is TileAtmosphere tile)
        {
            var tileRef = atmosphereSystem.GetTileRef(tile);
            var gridUid = tileRef.GridUid;
            var tilePos = tileRef.GridIndices;

            if (entityManager.TryGetComponent<MapGridComponent>(gridUid, out var gridComp) &&
                entityManager.TryGetComponent<GridAtmosphereComponent>(gridUid, out var gridAtmos))
            {
                radiationLevel = radiationSystem.GetRadiationAtCoordinates(entityManager.System<SharedMapSystem>().GridTileToLocal(gridUid, gridComp, tilePos));
            }
        }
        else if (holder is IComponent component)
        {
            radiationLevel = GetRadiationLevel(component.Owner);
        }
        else if (holder is PipeNet pipeNet)
        {
            int nodeCount = 0;

            foreach (var node in pipeNet.Nodes)
            {
                radiationLevel += GetRadiationLevel(node.Owner);
                nodeCount++;
            }

            if (nodeCount > 0 && radiationLevel > 0f)
                radiationLevel = radiationLevel / nodeCount;
        }

        if (radiationLevel < RadiationThreshold)
            return ReactionResult.Reacting; 

        float[] efficiencies = { radiationLevel, initCO2, initO2 * 2f };
        Array.Sort(efficiencies);
        float producedAmount = efficiencies[0];

        float co2Removed = producedAmount;
        float oxyRemoved = producedAmount * 0.5f;

        if (co2Removed > initCO2 ||
            oxyRemoved > initO2) 
            return ReactionResult.NoReaction;

        if (producedAmount <= 0) 
            return ReactionResult.Reacting;

        float pluoxProduced = producedAmount;
        float hydroProduced = producedAmount * 0.01f;

        mixture.AdjustMoles(Gas.CarbonDioxide, -co2Removed);
        mixture.AdjustMoles(Gas.Oxygen, -oxyRemoved);
        mixture.AdjustMoles(Gas.Pluoxium, pluoxProduced);
        mixture.AdjustMoles(Gas.Hydrogen, hydroProduced);

        float energyReleased = producedAmount * Atmospherics.PluoxiumProductionEnergy;
        float heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }

    private float GetRadiationLevel(EntityUid entity)
    {
        bool hadReceiver = entityManager.HasComponent<RadiationReceiverComponent>(entity);
        var receiverComp = entityManager.EnsureComponent<RadiationReceiverComponent>(entity);

        if (!hadReceiver)
        {
            var timerComp = entityManager.EnsureComponent<RadiationReceiverTimerComponent>(entity);
            timerComp.TimerExpiresAt = gameTiming.CurTime + TimerDuration;
        }
        else if (entityManager.TryGetComponent<RadiationReceiverTimerComponent>(entity, out var timerComp))
        {
            timerComp.TimerExpiresAt = gameTiming.CurTime + TimerDuration;
        }

        return receiverComp.CurrentRadiation;
    }

}

[RegisterComponent]
public sealed partial class RadiationReceiverTimerComponent : Component
{
    public TimeSpan TimerExpiresAt { get; set; } = TimeSpan.Zero;
}

public sealed partial class RadiationTimerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IEntityManager entityManager = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var timer in entityManager.EntityQuery<RadiationReceiverTimerComponent>())
        {
            var uid = timer.Owner; 
            if (_timing.CurTime >= timer.TimerExpiresAt)
            {
                entityManager.RemoveComponent<RadiationReceiverComponent>(uid);
                entityManager.RemoveComponent<RadiationReceiverTimerComponent>(uid);
            }
        }
    }
}