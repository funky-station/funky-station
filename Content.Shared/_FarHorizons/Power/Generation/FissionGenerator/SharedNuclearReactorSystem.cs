using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

public abstract class SharedNuclearReactorSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly ItemSlotsSystem _slotsSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Bound UI subscriptions
        SubscribeLocalEvent<NuclearReactorComponent, ReactorEjectItemMessage>(OnEjectItemMessage);
    }

    protected bool ReactorTryGetSlot(EntityUid uid, string slotID, out ItemSlot? itemSlot) => _slotsSystem.TryGetSlot(uid, slotID, out itemSlot);

    public virtual void UpdateGridVisual(EntityUid uid, NuclearReactorComponent? comp)
    {
        for (var x = 0; x < NuclearReactorComponent.ReactorGridWidth; x++)
        {
            for (var y = 0; y < NuclearReactorComponent.ReactorGridHeight; y++)
            {
                if(comp!.ComponentGrid[x, y] == null)
                {
                    _appearance.SetData(_entityManager.GetEntity(comp.VisualGrid[x, y]), ReactorCapVisuals.Sprite, ReactorCaps.Base);
                    continue;
                }
                else
                    _appearance.SetData(_entityManager.GetEntity(comp.VisualGrid[x, y]), ReactorCapVisuals.Sprite, ChoseSprite(comp.ComponentGrid[x,y]!.IconStateCap));
            }
        }
    }

    private static ReactorCaps ChoseSprite(string capName) => capName switch
    {
        "base_cap" => ReactorCaps.Base,

        "control_cap" => ReactorCaps.Control,
        "control_cap_melted_1" => ReactorCaps.ControlM1,
        "control_cap_melted_2" => ReactorCaps.ControlM2,
        "control_cap_melted_3" => ReactorCaps.ControlM3,
        "control_cap_melted_4" => ReactorCaps.ControlM4,

        "fuel_cap" => ReactorCaps.Fuel,
        "fuel_cap_melted_1" => ReactorCaps.FuelM1,
        "fuel_cap_melted_2" => ReactorCaps.FuelM2,
        "fuel_cap_melted_3" => ReactorCaps.FuelM3,
        "fuel_cap_melted_4" => ReactorCaps.FuelM4,

        "gas_cap" => ReactorCaps.Gas,
        "gas_cap_melted_1" => ReactorCaps.GasM1,
        "gas_cap_melted_2" => ReactorCaps.GasM2,
        "gas_cap_melted_3" => ReactorCaps.GasM3,
        "gas_cap_melted_4" => ReactorCaps.GasM4,

        "heat_cap" => ReactorCaps.Heat,
        "heat_cap_melted_1" => ReactorCaps.HeatM1,
        "heat_cap_melted_2" => ReactorCaps.HeatM2,
        "heat_cap_melted_3" => ReactorCaps.HeatM3,
        "heat_cap_melted_4" => ReactorCaps.HeatM4,

        _ => ReactorCaps.Base,
    };

    private void OnEjectItemMessage(EntityUid uid, NuclearReactorComponent component, ReactorEjectItemMessage args)
    {
        if (component.PartSlot.Item == null)
            return;

        _slotsSystem.TryEjectToHands(uid, component.PartSlot, args.Actor);
    }
}

public static class NuclearReactorPrefabs
{
    private static readonly ReactorPartComponent c = BaseReactorComponents.ControlRod;
    private static readonly ReactorPartComponent f = BaseReactorComponents.FuelRod;
    private static readonly ReactorPartComponent g = BaseReactorComponents.GasChannel;
    private static readonly ReactorPartComponent h = BaseReactorComponents.HeatExchanger;

    public static readonly ReactorPartComponent?[,] Empty =
    {
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        }
    };

    public static readonly ReactorPartComponent?[,] Normal =
    {
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        },
        {
            g, h, g, h, g, h, g
        },
        {
            h, null, c, null, c, null, h
        },
        {
            g, h, g, h, g, h, g
        },
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        }
    };

    public static readonly ReactorPartComponent?[,] Debug =
    {
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        },
        {
            g, h, g, h, g, h, g
        },
        {
            h, f, c, f, c, f, h
        },
        {
            g, h, g, h, g, h, g
        },
        {
            null, null, null, null, null, null, null
        },
        {
            null, null, null, null, null, null, null
        }
    };

    public static readonly ReactorPartComponent?[,] Meltdown =
    {
        {
            f, f, f, f, f, f, f
        },
        {
            f, f, f, f, f, f, f
        },
        {
            f, f, f, f, f, f, f
        },
        {
            f, f, f, f, f, f, f
        },
        {
            f, f, f, f, f, f, f
        },
        {
            f, f, f, f, f, f, f
        },
        {
            f, f, f, f, f, f, f
        },
    };

    public static readonly ReactorPartComponent?[,] Alignment =
    {
        {
            null, null, null, null, null, null, c
        },
        {
            null, null, null, null, null, c, null
        },
        {
            null, null, null, null, c, null, null
        },
        {
            null, null, null, c, null, null, null
        },
        {
            null, null, c, null, c, null, null
        },
        {
            null, c, null, null, null, c, null
        },
        {
            c, null, null, null, null, null, c
        }
    };
}