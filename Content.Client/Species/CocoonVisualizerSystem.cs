using System.Linq;
using Content.Shared.Clothing;
using Content.Shared.Humanoid;
using Content.Shared.Species.Arachnid;
using Robust.Client.GameObjects;

namespace Content.Client.Species;

public sealed class CocoonVisualizerSystem : VisualizerSystem<CocoonedComponent>
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CocoonedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CocoonedComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<EquipmentVisualsUpdatedEvent>(OnEquipmentVisualsUpdated);
    }

    private void OnEquipmentVisualsUpdated(EquipmentVisualsUpdatedEvent args)
    {
        // If the entity wearing the equipment is cocooned, hide the clothing layers immediately
        if (!HasComp<CocoonedComponent>(args.Equipee))
            return;

        if (!TryComp<SpriteComponent>(args.Equipee, out var sprite))
            return;

        // Hide all the layers that were just revealed by the clothing
        foreach (var layerKey in args.RevealedLayers)
        {
            if (SpriteSystem.LayerMapTryGet((args.Equipee, sprite), layerKey, out _, false))
            {
                SpriteSystem.LayerSetVisible((args.Equipee, sprite), layerKey, false);
            }
        }
    }

    private void SetupCocoonVisuals(Entity<CocoonedComponent> ent, SpriteComponent sprite)
    {
        // Reserve the layer if it doesn't exist
        // This will add the layer at the end, making it render on top of other layers (which is desired for a cocoon overlay)
        if (!SpriteSystem.LayerMapTryGet((ent, sprite), CocoonedKey.Key, out var cocoonLayerIndex, false))
        {
            cocoonLayerIndex = SpriteSystem.LayerMapReserve((ent, sprite), CocoonedKey.Key);
        }

        // Set the sprite from the component's data field
        SpriteSystem.LayerSetSprite((ent, sprite), CocoonedKey.Key, ent.Comp.Sprite);
        SpriteSystem.LayerSetVisible((ent, sprite), CocoonedKey.Key, true);

        // Hide all humanoid visual layers when cocooned
        if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid))
        {
            var allLayers = Enum.GetValues<HumanoidVisualLayers>();
            _humanoidAppearance.SetLayersVisibility(ent, allLayers, false, permanent: true, humanoid);
        }

        // Hide ALL sprite layers except the cocoon layer
        // This ensures clothing layers (which may be added dynamically) are also hidden
        for (var i = 0; i < sprite.AllLayers.Count(); i++)
        {
            if (i != cocoonLayerIndex)
            {
                sprite[i].Visible = false;
            }
        }
    }

    private void OnInit(Entity<CocoonedComponent> ent, ref ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        SetupCocoonVisuals(ent, sprite);
    }

    protected override void OnAppearanceChange(EntityUid uid, CocoonedComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        // Re-hide all layers when appearance changes (clothing system might have shown them again)
        SetupCocoonVisuals((uid, component), args.Sprite);
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

        // Restore all humanoid visual layers when uncocooned
        if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid))
        {
            var allLayers = Enum.GetValues<HumanoidVisualLayers>();
            _humanoidAppearance.SetLayersVisibility(ent, allLayers, true, permanent: true, humanoid);
        }

        // Restore all sprite layers visibility when uncocooned
        // The clothing system will handle showing/hiding based on what's equipped
        foreach (var layer in sprite.AllLayers)
        {
            layer.Visible = true;
        }
    }
}

