using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Mining;

/// <summary>
/// Claim an offer from the magnet UI.
/// </summary>
[Serializable, NetSerializable]
public sealed class MagnetClaimOfferEvent : BoundUserInterfaceMessage
{
    public int Index;
}
