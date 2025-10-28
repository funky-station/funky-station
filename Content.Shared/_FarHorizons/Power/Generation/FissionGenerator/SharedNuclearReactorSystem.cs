namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

public abstract class SharedNuclearReactorSystem : EntitySystem
{

    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    protected EntityUid[,] _reactorGrid = new EntityUid[NuclearReactorComponent.ReactorGridWidth, NuclearReactorComponent.ReactorGridHeight];

    protected virtual void UpdateGridVisual(EntityUid uid, NuclearReactorComponent? comp)
    {
        //if (!Resolve(uid, ref comp, ref appearance, false))
        //    return;

        for (var x = 0; x < NuclearReactorComponent.ReactorGridWidth; x++)
        {
            for (var y = 0; y < NuclearReactorComponent.ReactorGridHeight; y++)
            {
                if(comp!.ComponentGrid[x, y] == null)
                {
                    _appearance.SetData(_reactorGrid[x, y], ReactorCapVisuals.Sprite, ReactorCaps.Base);
                    continue;
                }
                else
                    _appearance.SetData(_reactorGrid[x,y], ReactorCapVisuals.Sprite, ChoseSprite(comp.ComponentGrid[x,y]!.IconStateCap));
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
}

public static class FissionGeneratorPrefabs
{
    private static readonly ReactorControlRodComponent c = BaseReactorComponents.ControlRod;
    private static readonly ReactorPartComponent f = BaseReactorComponents.FuelRod;
    private static readonly ReactorGasChannelComponent g = BaseReactorComponents.GasChannel;
    private static readonly ReactorPartComponent h = BaseReactorComponents.HeatExchanger;

    public static readonly ReactorPart?[,] Empty =
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

    public static readonly ReactorPart?[,] Normal =
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

    public static readonly ReactorPart?[,] Debug =
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
            h, f, c, null, c, f, h
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

    public static readonly ReactorPart?[,] Meltdown =
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

    public static readonly ReactorPart?[,] Alignment =
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