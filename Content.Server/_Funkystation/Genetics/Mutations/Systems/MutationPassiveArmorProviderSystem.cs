using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationArmorSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PassiveArmorProviderComponent, DamageModifyEvent>(OnDamageModify);
    }

    /// <summary>
    /// Applies the mutation's damage reduction.
    /// </summary>
    private void OnDamageModify(EntityUid uid, PassiveArmorProviderComponent comp, ref DamageModifyEvent args)
    {
        DamageModifierSet modifiers = _proto.TryIndex(comp.ModifierSetId, out var proto) ? proto : new();

        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, modifiers);
    }
}
