using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Shared._FarHorizons.PhysicalMaterial;

[Prototype]
public sealed partial class PhysicalMaterialPrototype : IPrototype, IInheritingPrototype
{
    [ViewVariables]
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<PhysicalMaterialPrototype>))]
    public string[]? Parents { get; private set; }

    [ViewVariables]
    [AbstractDataField]
    public bool Abstract { get; private set; } = false;

    [IdDataField, ViewVariables]
    public string ID { get; private set; } = default!;

    [DataField("name")]
    public LocId Name { get; private set; } = string.Empty;

    [DataField("description")]
    public LocId Description { get; private set; } = string.Empty;

    [DataField("color")]
    public Color Color { get; private set; } = Color.Lime;

    [DataField("properties")]
    public MaterialProperties Properties { get; private set; } = default!;
}

[DataDefinition]
public partial struct MaterialProperties()
{
    [DataField("electrical")]
    public float ElectricalConductivity { get; set; } = 5;

    [DataField("thermal")]
    public float ThermalConductivity { get; set; } = 5;

    [DataField("hard")]
    public float Hardness { get; set; } = 3;

    [DataField("density")]
    public float Density { get; set; } = 3;

    [DataField("reflective")]
    public float Reflectivity { get; set; } = 0;

    [DataField("flammable")]
    public float Flammability { get; set; } = 1;

    [DataField("chemical")]
    public float ChemicalResistance { get; set; } = 3;

    [DataField("radioactive")]
    public float Radioactivity { get; set; } = 0;

    [DataField("n_radioactive")]
    public float NeutronRadioactivity { get; set; } = 0;

    [DataField("spent_fuel")]
    public float FissileIsotopes { get; set; } = 0;

    [DataField("molitz_bubbles")]
    public float GasPockets { get; set; } = 0;

    [DataField("plasma_offgas")]
    public float ActivePlasma { get; set; } = 0;

    public MaterialProperties(MaterialProperties source) : this()
    {
        ElectricalConductivity = source.ElectricalConductivity;
        ThermalConductivity = source.ThermalConductivity;
        Hardness = source.Hardness;
        Density = source.Density;
        Reflectivity = source.Reflectivity;
        Flammability = source.Flammability;
        ChemicalResistance = source.ChemicalResistance;
        Radioactivity = source.Radioactivity;
        NeutronRadioactivity = source.NeutronRadioactivity;
        FissileIsotopes = source.FissileIsotopes;
        GasPockets = source.GasPockets;
        ActivePlasma = source.ActivePlasma;
    }
}