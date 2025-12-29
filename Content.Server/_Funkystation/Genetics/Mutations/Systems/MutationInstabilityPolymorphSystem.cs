using System.Linq;
using Content.Server._Funkystation.Genetics.Components;
using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Server._Funkystation.Genetics.Systems;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared._Funkystation.Genetics.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationInstabilityPolymorphSystem : EntitySystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly GeneticsSystem _genetics = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Trigger polymorph when the component is added
        SubscribeLocalEvent<MutationInstabilityPolymorphComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid oldUid, MutationInstabilityPolymorphComponent component, ComponentInit args)
    {
        // If no genetics, just try to polymorph and bail. This should never happen outside of testing.
        if (!TryComp<GeneticsComponent>(oldUid, out var oldGenetics))
        {
            _polymorph.PolymorphEntity(oldUid, component.PolymorphId);
            return;
        }

        var mutationSnapshot = oldGenetics.Mutations.Select(entry => entry).ToList();
        var enabledMutationIds = new HashSet<string>();
        foreach (var entry in oldGenetics.Mutations)
        {
            if (!entry.Enabled)
                continue;

            if (_proto.TryIndex<GeneticMutationPrototype>(entry.Id, out var proto))
            {
                bool addsPolymorphTrigger = proto.Components.Values
                    .Any(c => c.Component is MutationInstabilityPolymorphComponent);

                if (addsPolymorphTrigger)
                    continue;
            }

            enabledMutationIds.Add(entry.Id);
        }
        var instability = oldGenetics.GeneticInstability;
        var baseMutationIds = new HashSet<string>(oldGenetics.BaseMutationIds);

        var newUid = _polymorph.PolymorphEntity(oldUid, component.PolymorphId);

        if (!newUid.HasValue)
            return;

        // The old entity is now deleted, we're working on the new one

        // Restore GeneticsComponent with exact previous state
        var newGenetics = EnsureComp<GeneticsComponent>(newUid.Value);
        newGenetics.Mutations.Clear();
        foreach (var entry in mutationSnapshot)
        {
            newGenetics.Mutations.Add(entry);
        }
        newGenetics.BaseMutationIds = baseMutationIds;
        newGenetics.GeneticInstability = instability;

        // Re-enable all previously active mutations
        foreach (var mutationId in enabledMutationIds)
        {
            _genetics.TryDeactivateMutation(newUid.Value, newGenetics, mutationId);
            _genetics.TryActivateMutation(newUid.Value, newGenetics, mutationId);
        }

        if (HasComp<MutationInstabilityPolymorphComponent>(newUid))
            RemComp<MutationInstabilityPolymorphComponent>(newUid.Value);

        Dirty(newUid.Value, newGenetics);
    }
}
