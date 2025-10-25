namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

public abstract class SharedFissionGeneratorSystem : EntitySystem
{

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
}