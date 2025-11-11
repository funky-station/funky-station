namespace Content.Shared._FarHorizons.PhysicalMaterial.Systems;

public sealed class PhysicalMaterialSystem
{
    public static double CalculateHeatTransferCoefficient(MaterialProperties? materialA, MaterialProperties? materialB)
    {
        var hTC1 = 5.0;
        var hTC2 = 5.0;

        if (materialA != null)
            if (materialA.Value.ThermalConductivity > 0 && materialA.Value.ElectricalConductivity > 0)
                hTC1 = (Math.Max(materialA.Value.ThermalConductivity, 0) + Math.Max(materialA.Value.ElectricalConductivity, 0)) / 2;
            else if (materialA.Value.ThermalConductivity > 0)
                hTC1 = Math.Max(materialA.Value.ThermalConductivity, 0);
            else if (materialA.Value.ElectricalConductivity > 0)
                hTC1 = Math.Max(materialA.Value.ElectricalConductivity, 0);
        if (materialB != null)
            if (materialB.Value.ThermalConductivity > 0 && materialB.Value.ElectricalConductivity > 0)
                hTC2 = (Math.Max(materialB.Value.ThermalConductivity, 0) + Math.Max(materialB.Value.ElectricalConductivity, 0)) / 2;
            else if (materialB.Value.ThermalConductivity > 0)
                hTC2 = Math.Max(materialB.Value.ThermalConductivity, 0);
            else if (materialB.Value.ElectricalConductivity > 0)
                hTC2 = Math.Max(materialB.Value.ElectricalConductivity, 0);

        return ((Math.Pow(10, hTC1 / 5) - 1) + (Math.Pow(10, hTC2 / 5) - 1))/2;
    }
}