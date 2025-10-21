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
            return;

        if (!TryComp(ent.Owner, out TurbineComponent? comp))
            return;

        _popupSystem.PopupClient(Loc.GetString("turbine-repair", ("target", ent.Owner), ("tool", args.Used!)), ent.Owner, args.User);
    }
    }
