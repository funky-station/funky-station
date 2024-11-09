using Content.Shared.Clothing.Components;
using Content.Shared.Revolutionary;
using Robust.Shared.Timing;

namespace Content.Server.Revolutionary;

public sealed class RevolutionaryClothingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        SubscribeLocalEvent<RevolutionaryClothingComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, RevolutionaryClothingComponent component, ComponentInit init)
    {
        var timer = new Stopwatch();

        if (!TryComp<ClothingComponent>(uid, out var clothingComponent))
            return;


    }
}
