using System.Linq;
using Content.Shared.Atmos;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Cargo.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class GasMinerConsoleComponent : Component
{
    /// <summary>
    /// List of all currently linked gas miners.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntityUid> LinkedMiners = new();

    /// <summary>
    /// Current amount of gas mining credits available on this console.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Credits = 10000f;

    /// <summary>
    /// Whether or not to automatically purchase gas credits when they run low.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AutoBuy = true;
}

[Serializable, NetSerializable]
public sealed class GasMinerSetSettingsMessage : BoundUserInterfaceMessage
{
    public readonly int MinerIndex;
    public readonly float NewSpawnAmount;
    public readonly float NewMaxExternalPressure;

    public GasMinerSetSettingsMessage(int minerIndex, float newSpawnAmount, float newMaxExternalPressure)
    {
        MinerIndex = minerIndex;
        NewSpawnAmount = newSpawnAmount;
        NewMaxExternalPressure = newMaxExternalPressure;
    }
}

[Serializable, NetSerializable]
public sealed class BuyGasCreditsMessage : BoundUserInterfaceMessage
{
    public int Amount { get; }

    public BuyGasCreditsMessage(int amount)
    {
        Amount = amount;
    }
}

[Serializable, NetSerializable]
public sealed class AutoBuyToggleMessage : BoundUserInterfaceMessage
{
    public bool Enabled { get; }
    public AutoBuyToggleMessage(bool enabled)
    {
        Enabled = enabled;
    }
}
