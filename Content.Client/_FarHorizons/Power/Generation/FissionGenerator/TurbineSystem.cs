using Content.Client.Examine;
using Content.Client.Popups;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Repairable;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client._FarHorizons.Power.Generation.FissionGenerator;

// Ported and modified from goonstation by Jhrushbe.
// CC-BY-NC-SA-3.0
// https://github.com/goonstation/goonstation/blob/master/code/obj/nuclearreactor/turbine.dm

public sealed class TurbineSystem : SharedTurbineSystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    private static readonly EntProtoId ArrowPrototype = "TurbineFlowArrow";

    public override void Initialize()
    {
        SubscribeLocalEvent<TurbineComponent, ClientExaminedEvent>(ReactorExamined);
    }

    protected override void UpdateUI(EntityUid uid, TurbineComponent turbine)
    {
        if (_userInterfaceSystem.TryGetOpenUi(uid, TurbineUiKey.Key, out var bui))
        {
            bui.Update();
        }
    }
    protected override void OnRepairTurbineFinished(
    Entity<TurbineComponent> ent,
    ref SharedRepairableSystem.RepairFinishedEvent args)

    {
        if (args.Cancelled)
            return;

        if (!TryComp(ent.Owner, out TurbineComponent? comp))
            return;

        _popupSystem.PopupClient(Loc.GetString("turbine-repair", ("target", ent.Owner), ("tool", args.Used!)), ent.Owner, args.User);
    }

    private void ReactorExamined(EntityUid uid, TurbineComponent comp, ClientExaminedEvent args)
    {
        Spawn(ArrowPrototype, new EntityCoordinates(uid, 0, 0));
    }
}
