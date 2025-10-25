using Content.Shared.Atmos;
using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

[RegisterComponent, NetworkedComponent]
public sealed partial class FissionGeneratorComponent : Component
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

    // Making this a DataField causes the game to explode, neat
    public ReactorPart?[,] ComponentGrid;

    [DataField]
    public string Prefab = "debug";
    [DataField]
    public bool ApplyPrefab = true;

    [DataField("inlet")]
    public string InletName { get; set; } = "inlet";

    [DataField("outlet")]
    public string OutletName { get; set; } = "outlet";

    [DataField("[DEBUG] neutrons")]
    public int NeutronCount = 0;
}