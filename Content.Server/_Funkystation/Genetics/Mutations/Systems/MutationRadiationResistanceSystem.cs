using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Shared.Damage.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

/// <remarks>
///     This has been copied from RadiationProtectionSystem to prevent component conflicts with mutation enabling/disabling
/// </remarks>
public sealed class MutationRadiationResistanceSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationRadiationResistanceComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MutationRadiationResistanceComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(EntityUid uid, MutationRadiationResistanceComponent component, ComponentInit args)
    {
        if (!_prototypeManager.TryIndex(component.ModifierSetId, out var modifier))
            return;

        var buffComp = EnsureComp<DamageProtectionBuffComponent>(uid);
        // add the damage modifier if it isn't already present
        if (!buffComp.Modifiers.ContainsKey(component.ModifierSetId))
            buffComp.Modifiers.Add(component.ModifierSetId, modifier);
    }

    private void OnShutdown(EntityUid uid, MutationRadiationResistanceComponent component, ComponentShutdown args)
    {
        if (!TryComp<DamageProtectionBuffComponent>(uid, out var buffComp))
            return;

        buffComp.Modifiers.Remove(component.ModifierSetId);

        if (buffComp.Modifiers.Count == 0)
            RemComp<DamageProtectionBuffComponent>(uid);
    }
}
