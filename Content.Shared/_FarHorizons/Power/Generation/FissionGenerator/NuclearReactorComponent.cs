using Content.Shared.Atmos;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

[RegisterComponent, NetworkedComponent]
public sealed partial class NuclearReactorComponent : Component
{
    public static int ReactorGridWidth = 7;
    public static int ReactorGridHeight = 7;
    public readonly int ReactorOverheatTemp = 1200;
    public readonly int ReactorFireTemp = 1500;
    public readonly int ReactorMeltdownTemp = 2000;
    
    [DataField]
    public float RadiationLevel = 0;
    [DataField]
    public float ReactorVesselGasVolume = 200;
    [DataField]
    public bool Melted = false;
    [DataField]
    public float Temperature = Atmospherics.T20C;
    [DataField]
    public float ThermalMass = 420 * 2000; // specific heat capacity of steel (420 J/KgK) * mass of reactor (Kg)
    [DataField]
    public float ControlRodInsertion = 2;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField]
    public float AvgInsertion = 0;

    public SoundSpecifier MeltdownSound = new SoundPathSpecifier("/Audio/_FarHorizons/Machines/meltdown_siren.ogg");

    [DataField]
    public bool IsSmoking = false;
    [DataField]
    public bool IsBurning = false;
    [DataField]
    public string AlertChannel = "Engineering";

    [DataField]
    public string MeltdownAlertLevel = "yellow";

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField]
    public float ThermalPower = 0;
    public float[] ThermalPowerL1 = new float[32];
    public float[] ThermalPowerL2 = new float[32];

    public EntityUid? AlarmAudioHighThermal;
    public EntityUid? AlarmAudioHighTemp;
    public EntityUid? AlarmAudioHighRads;

    [DataField]
    public ItemSlot PartSlot = new();

    // Making this a DataField causes the game to explode, neat
    public ReactorPartComponent?[,] ComponentGrid = new ReactorPartComponent[ReactorGridWidth, ReactorGridHeight];

    // Woe, 3 dimensions be upon ye
    public List<ReactorNeutron>[,] FluxGrid = new List<ReactorNeutron>[ReactorGridWidth, ReactorGridHeight];

    public double[,] TemperatureGrid = new double[ReactorGridWidth, ReactorGridHeight];
    public int[,] NeutronGrid = new int[ReactorGridWidth, ReactorGridHeight];

    public NetEntity[,] VisualGrid = new NetEntity[ReactorGridWidth, ReactorGridHeight];

    public GasMixture? AirContents;

    [DataField]
    public string Prefab = "normal";
    [DataField]
    public bool ApplyPrefab = true;

    [DataField("inlet")]
    public string InletName { get; set; } = "inlet";

    [DataField("outlet")]
    public string OutletName { get; set; } = "outlet";

    #region Debug
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("neutrons")]
    public int NeutronCount = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("meltedParts")]
    public int MeltedParts = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("controlRods")]
    public int DetectedControlRods = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("totalN-Rads")]
    public float TotalNRads = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("totalRads")]
    public float TotalRads = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("spentFuel")]
    public float TotalSpent = 0;
    #endregion
}