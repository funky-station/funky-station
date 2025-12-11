using System.Numerics;
using Content.Shared._Funkystation.Genetics.Components;
using Robust.Client.GameObjects;

namespace Content.Client._Funkystation.Genetics.Systems;

public sealed class DnaScannerVisualSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InsideDnaScannerComponent, ComponentStartup>(OnInserted);
        SubscribeLocalEvent<InsideDnaScannerComponent, ComponentShutdown>(OnRemoved);
    }

    private void OnInserted(EntityUid uid, InsideDnaScannerComponent component, ComponentStartup args)
    {
        if (TryComp<SpriteComponent>(uid, out var sprite))
            sprite.Visible = false;
    }

    private void OnRemoved(EntityUid uid, InsideDnaScannerComponent component, ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(uid, out var sprite))
            sprite.Visible = true;
    }
}
