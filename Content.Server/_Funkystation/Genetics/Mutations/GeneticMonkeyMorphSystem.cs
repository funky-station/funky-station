namespace Content.Server._Funkystation.Genetics.Systems;

using Content.Server._Funkystation.Genetics.Components;
using Content.Server.Polymorph.Systems;

public sealed class GeneticMonkeyMorphSystem : EntitySystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GeneticMonkeyMorphComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, GeneticMonkeyMorphComponent component, ComponentInit args)
    {
        const string monkeyPolymorphPrototype = "AdminMonkeySmite";

        _polymorph.PolymorphEntity(uid, monkeyPolymorphPrototype);

        RemCompDeferred<GeneticMonkeyMorphComponent>(uid);
    }
}
