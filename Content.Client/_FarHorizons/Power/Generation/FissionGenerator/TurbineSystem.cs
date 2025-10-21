using Content.Client.Popups;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Repairable;
using Robust.Client.GameObjects;

namespace Content.Client._FarHorizons.Power.Generation.FissionGenerator;

public sealed class TurbineSystem : SharedTurbineSystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    protected override void UpdateUi(Entity<TurbineComponent> entity)
    {
        if (_userInterfaceSystem.TryGetOpenUi(entity.Owner, TurbineUiKey.Key, out var bui))
        {
            bui.Update();
        }
    }

    protected override void OnRepairTurbineFinished(Entity<TurbineComponent> ent, ref RepairFinishedEvent args)
    {
        if (args.Cancelled)
        {
            _popupSystem.PopupClient(Loc.GetString("nope-cancelled"), ent.Owner, args.User);
            return;
        }

        if (!TryComp(ent.Owner, out TurbineComponent? comp))
        {
            _popupSystem.PopupClient(Loc.GetString("nope-no-turbine"), ent.Owner, args.User);
            return;
        }

        if (comp.Ruined)
        {
            _popupSystem.PopupClient(Loc.GetString("turbine-repair-ruined", ("target", ent.Owner), ("tool", args.Used!)), ent.Owner, args.User);
            return;
        }
        else if (comp.BladeHealth < comp.BladeHealthMax)
        {
            _popupSystem.PopupClient(Loc.GetString("turbine-repair", ("target", ent.Owner), ("tool", args.Used!)), ent.Owner, args.User);
            return;
        }
        else if (comp.BladeHealth >= comp.BladeHealthMax)
        {
            // This should technically never occur, but just in case...
            _popupSystem.PopupClient(Loc.GetString("turbine-no-damage", ("target", comp.BladeHealth), ("tool", comp.BladeHealthMax)), ent.Owner, args.User);
            return;
        }
        _popupSystem.PopupClient(Loc.GetString("ye-it-broken-lol"), ent.Owner, args.User);
    }
}
