using Content.Shared.Species.Arachnid;
using Robust.Client.GameObjects;

namespace Content.Client.Species;

public sealed class CocoonVisualizerSystem : VisualizerSystem<CocoonedComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CocoonedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CocoonedComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<CocoonedComponent> ent, ref ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        // Reserve the layer if it doesn't exist
        // This will add the layer at the end, making it render on top of other layers (which is desired for a cocoon overlay)
        if (!SpriteSystem.LayerMapTryGet((ent, sprite), CocoonedKey.Key, out _, false))
        {
            SpriteSystem.LayerMapReserve((ent, sprite), CocoonedKey.Key);
        }

        // Set the sprite from the component's data field
        // TODO: Replace the placeholder sprite in CocoonedComponent with your actual cocoon sprite
        // The sprite path is defined in CocoonedComponent.Sprite field
        SpriteSystem.LayerSetSprite((ent, sprite), CocoonedKey.Key, ent.Comp.Sprite);
        SpriteSystem.LayerSetVisible((ent, sprite), CocoonedKey.Key, true);
    }

    private void OnShutdown(Entity<CocoonedComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        // Hide the cocoon layer when the component is removed
        if (SpriteSystem.LayerMapTryGet((ent, sprite), CocoonedKey.Key, out _, false))
        {
            SpriteSystem.LayerSetVisible((ent, sprite), CocoonedKey.Key, false);
        }
    }
}
