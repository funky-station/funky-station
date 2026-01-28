using Content.Server._Funkystation.Genetics.Mutations.Components;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

using Content.Shared.Temperature;

public sealed class MutationThermalResistanceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MutationThermalResistanceComponent, GetThermalInsulationEvent>(OnGetInsulation);
        SubscribeLocalEvent<MutationThermalResistanceComponent, ModifyChangedTemperatureEvent>(OnModifyTemperature);
    }

    private void OnGetInsulation(EntityUid uid, MutationThermalResistanceComponent component, ref GetThermalInsulationEvent args)
    {
        var coefficient = args.TemperatureDelta < 0
            ? component.CoolingCoefficient
            : component.HeatingCoefficient;

        args.Coefficient *= coefficient;
    }

    private void OnModifyTemperature(EntityUid uid, MutationThermalResistanceComponent component, ref ModifyChangedTemperatureEvent args)
    {
        var ev = new GetThermalInsulationEvent(1f)
        {
            TemperatureDelta = args.TemperatureDelta
        };

        RaiseLocalEvent(uid, ref ev);
        args.TemperatureDelta *= ev.Coefficient;
    }
}

[ByRefEvent]
public struct GetThermalInsulationEvent
{
    public float Coefficient;

    public float TemperatureDelta;

    public GetThermalInsulationEvent(float coefficient)
    {
        Coefficient = coefficient;
        TemperatureDelta = 0f;
    }
}
