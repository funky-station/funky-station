using System.Runtime.Intrinsics.X86;
using Content.Shared.Atmos;
using Content.Shared.Tools;
using Robust.Shared.Prototypes;

namespace Content.Server.Power.Generation.FissionGenerator;

[RegisterComponent]
public sealed partial class TurbineComponent : Component
{
    // Power generated last tick
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("lastGen")]
    public float LastGen = 0;

    // Watts per revolution
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("statorLoad")]
    public float StatorLoad = 35000;

    // Current RPM of turbine
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("RPM")]
    public float RPM = 0;

    // Turbine's resistance to change in RPM
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("turbineMass")]
    public float TurbineMass = 1000;

    // Most efficient power generation at this value, overspeed at 1.2*this
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("bestRPM")]
    public float BestRPM = 600;

    // Volume of gas to process per tick for power generation
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("flowRate")]
    public float FlowRate = 200;

    // Maximum volume of gas to process per tick
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("flowRateMax")]
    public float FlowRateMax = Atmospherics.MaxTransferRate * 5;

    // Max/min temperatures
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("maxTemp")]
    public float MaxTemp = 3000;
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("minTemp")]
    public float MinTemp = Atmospherics.T20C;

    // Health of the turbine
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("bladeHealth")]
    public int BladeHealth = 15;
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("bladeHealthMax")]
    public int BladeHealthMax = 15;

    // If the turbine is functional or not
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("ruined")]
    public bool Ruined = false;

    // Flag for indicating that energy available is less than needed to turn the turbine
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("stalling")]
    public bool Stalling = false;

    // Flag for RPM being > BestRPM*1.2
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("overspeed")]
    public bool Overspeed = false;

    // Flag for gas tempurature being > MaxTemp
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("overtemp")]
    public bool Overtemp = false;

    // Flag for gas tempurature being < MinTemp
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("undertemp")]
    public bool Undertemp = false;

    // Adjustment for power generation
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("powerMultiplier")]
    public float PowerMultiplier = 1;

    public float RepairDelay = 10;
    public float RepairFuelCost = 20;
    public ProtoId<ToolQualityPrototype> RepairTool = "Welding";

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("inlet")]
    public string InletName { get; set; } = "inlet";

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("outlet")]
    public string OutletName { get; set; } = "outlet";

    public bool IsSparking = false;
    public bool IsSmoking = false;

    [DataField, AutoNetworkedField]
    public List<EntityUid?> AudioStreams = [new(), new()]; 

    public List<string> DamageSoundList = [ 
        "/Audio/_FarHorizons/Effects/engine_grump1.ogg", 
        "/Audio/_FarHorizons/Effects/engine_grump2.ogg", 
        "/Audio/_FarHorizons/Effects/engine_grump3.ogg", 
        "/Audio/Effects/metal_slam5.ogg", 
        "/Audio/Effects/metal_scrape2.ogg" 
    ];

    //Debugging
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("HasPipes")]
    public bool HasPipes = false;
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("SupplierMaxSupply")]
    public float SupplierMaxSupply = 0;
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("LastVolumeTransfer")]
    public float LastVolumeTransfer = 0;

}
