using Content.Server.Audio;
using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Power.Nodes;
using Content.Server.Station.Systems;
using Content.Shared.Cargo.Components;
using Content.Shared.Examine;
using Content.Shared.NodeContainer;
using Content.Shared.Power;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using PowerTransmissionComponent = Content.Server._NF.Power.Components.PowerTransmissionComponent;
using Content.Shared.Cargo.Prototypes;

namespace Content.Server._NF.Power.EntitySystems;

/// <summary>
/// Handles logic for the PowerTransmissionComponent.
/// Consumes power, pays a bank account depending on the amount of power consumed.
/// </summary>
public sealed class PowerTransmissionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AmbientSoundSystem _ambientSound = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly CargoSystem _cargo = default!;
    [Dependency] private readonly NodeContainerSystem _node = default!;
    [Dependency] private readonly NodeGroupSystem _nodeGroup = default!;
    [Dependency] private readonly PointLightSystem _pointLight = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(PowerNetSystem));

        SubscribeLocalEvent<PowerTransmissionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PowerTransmissionComponent, ExaminedEvent>(OnExamined);
    }

    private void OnMapInit(Entity<PowerTransmissionComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextDeposit = _timing.CurTime + ent.Comp.DepositPeriod;
        if (TryComp(ent, out PowerConsumerComponent? power))
            power.DrawRate = Math.Clamp(power.DrawRate, ent.Comp.MinimumRequestablePower, ent.Comp.MaximumRequestablePower);
    }

    private void OnExamined(Entity<PowerTransmissionComponent> ent, ref ExaminedEvent args)
    {
        if (TryComp(ent, out PowerConsumerComponent? power))
        {
            args.PushMarkup(Loc.GetString("power-transmission-examine", 
                ("actual", power.ReceivedPower), 
                ("requested", power.DrawRate)));

            var powered = power.NetworkLoad.Enabled && power.NetworkLoad.ReceivingPower > 0;
            args.PushMarkup(
                Loc.GetString("power-receiver-component-on-examine-main",
                    ("stateText", Loc.GetString(powered
                        ? "power-receiver-component-on-examine-powered"
                        : "power-receiver-component-on-examine-unpowered"))
                )
            );
        }
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<PowerTransmissionComponent, PowerConsumerComponent>();
        while (query.MoveNext(out var uid, out var xmit, out var power))
        {
            // Machine on? Add power.
            if (power.NetworkLoad.Enabled)
                xmit.AccumulatedEnergy += power.NetworkLoad.ReceivingPower * frameTime;

            // If our time window has elapsed, scale your energy based on average power
            if (_timing.CurTime >= xmit.NextDeposit)
            {
                xmit.NextDeposit += xmit.DepositPeriod;

                if (!float.IsFinite(xmit.AccumulatedEnergy) || !float.IsPositive(xmit.AccumulatedEnergy))
                {
                    xmit.AccumulatedEnergy = 0.0f;
                    continue;
                }

                float totalPeriodSeconds = (float)xmit.DepositPeriod.TotalSeconds;
                float averagePower = xmit.AccumulatedEnergy / totalPeriodSeconds;
                float depositValue = GetPowerPayRate((uid, xmit), averagePower) * totalPeriodSeconds;

                xmit.AccumulatedEnergy = 0.0f;
                var depositSpesos = (int)depositValue;
                
                if (depositSpesos > 0)
                {
                    // Get the station entity for this power transmission device
                    var station = _station.GetOwningStation(uid);
                    if (station != null)
                    {
                        // Get the bank account component (nullable)
                        var bankAccount = CompOrNull<StationBankAccountComponent>(station.Value);
                        
                        // Use the CargoSystem to deposit funds to the station's bank account
                        _cargo.UpdateBankAccount(
                            (station.Value, bankAccount),
                            depositSpesos,
                            new ProtoId<CargoAccountPrototype>(xmit.Account) 

                        );
                    }
                }
            }

            bool powered = power.NetworkLoad.Enabled && power.NetworkLoad.ReceivingPower > 0;
            if (powered != xmit.LastPowered)
            {
                _appearance.SetData(uid, PowerDeviceVisuals.Powered, powered);
                _pointLight.SetEnabled(uid, powered);
                _ambientSound.SetAmbience(uid, powered);
                xmit.LastPowered = powered;
            }
        }
    }

    /// <summary>
    /// Gets the expected pay rate, in spesos per second.
    /// </summary>
    /// <param name="power">Input power level, in watts</param>
    /// <returns>Expected power sale value in spesos per second</returns>
    public float GetPowerPayRate(Entity<PowerTransmissionComponent> ent, float power)
    {
        if (!float.IsFinite(power) || !float.IsPositive(power))
        {
            return 0f;
        }

        float depositValue;
        if (power <= ent.Comp.LinearMaxValue)
            depositValue = ent.Comp.LinearRate * power;
        else
            depositValue = ent.Comp.LogarithmCoefficient * MathF.Pow(
                ent.Comp.LogarithmRateBase, 
                MathF.Log10(power) - ent.Comp.LogarithmSubtrahend);

        return MathF.Min(depositValue, ent.Comp.MaxValuePerSecond);
    }
}