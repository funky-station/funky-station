using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.Piping.Components;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Power.Components;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Interaction;
using Content.Shared.Power;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using static Content.Shared.Atmos.Components.GasAnalyzerComponent;

namespace Content.Server.Atmos.EntitySystems;

public sealed class GasPipeSensorSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly GasAnalyzerSystem _gasAnalyzer = default!;
    [Dependency] private readonly AtmosphereSystem _atmo = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GasPipeSensorComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GasPipeSensorComponent, AtmosDeviceUpdateEvent>(OnAtmosUpdate);
        SubscribeLocalEvent<GasPipeSensorComponent, ActivatableUIOpenAttemptEvent>(OnUiOpen);
        SubscribeLocalEvent<GasPipeSensorComponent, BoundUIClosedEvent>(OnUiClosed);
    }

    public override void Update(float frameTime)
    {
        var query = EntityManager.EntityQueryEnumerator<GasPipeSensorComponent, ActiveGasPipeSensorComponent>();

        while (query.MoveNext(out var uid, out var sensor, out var active))
        {
            active.AccumulatedFrameTime += frameTime;

            if (active.AccumulatedFrameTime < active.UpdateInterval)
                continue;

            active.AccumulatedFrameTime -= active.UpdateInterval;
            UpdateAndSendUi((uid, sensor));
        }
    }

    private void OnStartup(Entity<GasPipeSensorComponent> ent, ref ComponentStartup args)
    {
        UpdatePressureAppearance(ent);
    }

    private void OnAtmosUpdate(Entity<GasPipeSensorComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        UpdatePressureAppearance(ent);
    }

    private void OnUiOpen(Entity<GasPipeSensorComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (!TryComp<ApcPowerReceiverComponent>(ent, out var power) || !power.Powered)
        {
            args.Cancel();
            return;
        }

        UpdateAndSendUi(ent);

        // Start ticking
        EnsureComp<ActiveGasPipeSensorComponent>(ent);
    }

    private void OnUiClosed(Entity<GasPipeSensorComponent> ent, ref BoundUIClosedEvent args)
    {
        if (args.UiKey is GasPipeSensorUiKey.Key)
        {
            RemCompDeferred<ActiveGasPipeSensorComponent>(ent);
        }
    }

    private void UpdatePressureAppearance(Entity<GasPipeSensorComponent> ent)
    {
        PipeLightsState lightsState = PipeLightsState.Off;

        bool isPowered = false;
        if (TryComp<ApcPowerReceiverComponent>(ent, out var power))
            isPowered = power.Powered;

        if (isPowered &&
            TryComp<NodeContainerComponent>(ent, out var nodes) &&
            nodes.Nodes.TryGetValue("monitored", out var node) &&
            node is PipeNode pipe &&
            pipe.Air.Volume > 0)
        {
            var pressureKpa = pipe.Air.Pressure;

            if (pressureKpa >= 9000f)
                lightsState = PipeLightsState.ExtremePressure;
            else if (pressureKpa >= 4500f)
                lightsState = PipeLightsState.OverPressure;
            else
                lightsState = PipeLightsState.NormalPressure;
        }

        _appearance.SetData(ent, GasPipeSensorVisuals.LightsState, lightsState);
        Dirty(ent);
    }

    public void UpdateAndSendUi(Entity<GasPipeSensorComponent> ent)
    {
        if (!TryComp<NodeContainerComponent>(ent, out var nodes) ||
            !nodes.Nodes.TryGetValue("monitored", out var node) ||
            node is not PipeNode pipe ||
            pipe.Air.Volume <= 0)
        {
            var emptyEntry = new GasMixEntry(
                name: "Pipe",
                volume: 200f,
                pressure: 0f,
                temperature: 0f,
                gases: Array.Empty<GasEntry>()
            );

            var emptyMsg = new GasPipeSensorUserMessage(
                emptyEntry,
                "Pipe Sensor",
                200f,
                0f,
                error: "No valid pipe mixture available"
            );
            _ui.ServerSendUiMessage(ent.Owner, GasPipeSensorUiKey.Key, emptyMsg);
            return;
        }

        var mix = pipe.Air.Clone();
        mix.Multiply(pipe.Volume / pipe.Air.Volume);
        mix.Volume = pipe.Volume;

        var entry = new GasMixEntry(
            "Pipe",
            mix.Volume,
            mix.Pressure,
            mix.Temperature,
            _gasAnalyzer.GenerateGasEntryArray(mix));

        var msg = new GasPipeSensorUserMessage(
            entry,
            "Pipe Sensor",
            mix.Pressure,
            mix.Temperature);

        _ui.ServerSendUiMessage(ent.Owner, GasPipeSensorUiKey.Key, msg);
    }
}
