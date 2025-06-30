using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Mining;

[Serializable, NetSerializable]
public sealed class MiningMagnetBoundUserInterfaceState : BoundUserInterfaceState
{
    public TimeSpan? EndTime;
    public TimeSpan NextOffer;

    public TimeSpan Cooldown;
    public TimeSpan Duration;

    public int ActiveSeed;

    public List<int> Offers;

    public MiningMagnetBoundUserInterfaceState(List<int> offers)
    {
        Offers = offers;
    }
}
