using Content.Shared._Impstation.CosmicCult.Components;
using Content.Shared.Actions;

namespace Content.Shared._Impstation.CosmicCult;

internal sealed class RogueAscendedDendriteSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<RogueAscendedDendriteComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<RogueAscendedDendriteComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, RogueAscendedDendriteComponent component, ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, ref component.ActionEntity, component.Action);

        if (!TryComp<CosmicCultComponent>(uid, out var cultComp))
            return;

        cultComp.EntropyBudget += 30; // if they're a cultist, make them very very rich
        cultComp.CosmicEmpowered = true; // also empower them, assuming they aren't already
    }

    private void OnShutdown(EntityUid uid, RogueAscendedDendriteComponent component, ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(uid, component.ActionEntity);
    }
}
