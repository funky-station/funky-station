using Content.Shared.Clothing;
using Content.Shared.Humanoid;
using Content.Shared.Species.Arachnid;
using Robust.Client.GameObjects;

namespace Content.Client.Species;

public sealed class CocoonVisualizerSystem : VisualizerSystem<CocoonedComponent>
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    // List of clothing layer names that we hide when cocooned
    private static readonly string[] ClothingLayers = new[]
    {
        "jumpsuit",
        "shoes",
        "gloves",
        "ears",
        "outerClothing",
        "eyes",
        "belt",
        "id",
        "back",
        "neck",
        "mask",
        "head",
        "pocket1",
        "pocket2"
    };

    // List of humanoid body appearance layers to hide (excluding special layers like StencilMask, Handcuffs, etc.)
    private static readonly HumanoidVisualLayers[] BodyLayers = new[]
    {
        HumanoidVisualLayers.Special,
        HumanoidVisualLayers.Tail,
        HumanoidVisualLayers.Wings,
        HumanoidVisualLayers.Hair,
        HumanoidVisualLayers.FacialHair,
        HumanoidVisualLayers.Chest,
        HumanoidVisualLayers.Head,
        HumanoidVisualLayers.Snout,
        HumanoidVisualLayers.HeadSide,
        HumanoidVisualLayers.HeadTop,
        HumanoidVisualLayers.Eyes,
        HumanoidVisualLayers.RArm,
        HumanoidVisualLayers.LArm,
        HumanoidVisualLayers.RHand,
        HumanoidVisualLayers.LHand,
        HumanoidVisualLayers.RLeg,
        HumanoidVisualLayers.LLeg,
        HumanoidVisualLayers.RFoot,
        HumanoidVisualLayers.LFoot,
    };

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
        if (!SpriteSystem.LayerMapTryGet((ent, sprite), CocoonedKey.Key, out _, false))
        {
            SpriteSystem.LayerMapReserve((ent, sprite), CocoonedKey.Key);
        }

        // Set the sprite from the component's data field
        SpriteSystem.LayerSetSprite((ent, sprite), CocoonedKey.Key, ent.Comp.Sprite);
        SpriteSystem.LayerSetVisible((ent, sprite), CocoonedKey.Key, true);

        // Hide only body appearance layers when cocooned (not special layers like StencilMask, Handcuffs, etc.)
        if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid))
        {
            _humanoidAppearance.SetLayersVisibility(ent, BodyLayers, false, permanent: true, humanoid);
        }

        // Hide only clothing layers that exist and are visible
        foreach (var layerName in ClothingLayers)
        {
            if (SpriteSystem.LayerMapTryGet((ent, sprite), layerName, out var index, false))
            {
                // Check if the layer is visible before hiding it
                if (sprite[index].Visible)
                {
                    SpriteSystem.LayerSetVisible((ent, sprite), layerName, false);
                }
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

        // Re-hide clothing layers when appearance changes (clothing system might have shown them again)
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

        // Restore only the body appearance layers we hid (remove from PermanentlyHidden)
        // This will allow the humanoid appearance system to handle visibility normally
        if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid))
        {
            _humanoidAppearance.SetLayersVisibility(ent, BodyLayers, true, permanent: true, humanoid);
        }

        // Restore clothing layers by iterating over ClothingLayers and making them visible
        foreach (var layerName in ClothingLayers)
        {
            if (SpriteSystem.LayerMapTryGet((ent, sprite), layerName, out _, false))
            {
                SpriteSystem.LayerSetVisible((ent, sprite), layerName, true);
            }
        }

        // Trigger an appearance update to refresh the clothing system
        // This ensures clothing layers are properly shown based on what's equipped
        if (TryComp<AppearanceComponent>(ent, out var appearance))
        {
            _appearance.QueueUpdate(ent, appearance);
        }
    }
}
