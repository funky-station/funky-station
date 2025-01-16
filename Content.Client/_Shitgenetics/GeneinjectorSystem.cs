using System.Linq;
using Content.Client.DamageState;
using Content.Shared.Genetics;
using Content.Shared.Genetics.Components;
using Content.Shared.Humanoid.Markings;
using Robust.Client.GameObjects;

namespace Content.Client._Shitgenetics;

public sealed class GeneinjectorSystem : SharedGeneSystem
{

}

public sealed class GeneinjectorVisualizerSystem : VisualizerSystem<GeneinjectorComponent>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GeneinjectorComponent, AfterAutoHandleStateEvent>(OnGeneHandleState);
    }

    private void UpdateAppearance(EntityUid id, GeneinjectorComponent gene, AppearanceComponent? appearance = null, SpriteComponent? sprite = null)
    {
        if (!Resolve(id, ref appearance, ref sprite))
            return;

        sprite.LayerSetColor(0, gene.Color); //fucking hardcoded lmfao
    }

    protected override void OnAppearanceChange(EntityUid uid, GeneinjectorComponent component, ref AppearanceChangeEvent args)
    {
        UpdateAppearance(uid, component, args.Component, args.Sprite);
    }

    private void OnGeneHandleState(EntityUid uid, GeneinjectorComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateAppearance(uid, component);
    }
}
