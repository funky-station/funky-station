using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Robust.Client.GameObjects;

namespace Content.Client._FarHorizons.Power.Generation.FissionGenerator;

public sealed class TurbineSystem : SharedTurbineSystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    protected override void UpdateUi(Entity<TurbineComponent> entity)
    {
        if (_userInterfaceSystem.TryGetOpenUi(entity.Owner, TurbineUiKey.Key, out var bui))
        {
            bui.Update();
        }
    }
}
