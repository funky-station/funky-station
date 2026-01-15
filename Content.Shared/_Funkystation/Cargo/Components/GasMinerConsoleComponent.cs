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
    /// Multiplier applied to gas purchases made through the console.
    /// </summary>
    [DataField]
    public float PriceMultiplier = 1.0f;
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
public sealed class BuyMolesForMinerMessage : BoundUserInterfaceMessage
{
    public int MinerIndex { get; }
    public int SpecoAmount { get; }
    public BuyMolesForMinerMessage(int minerIndex, int specoAmount)
    {
        MinerIndex = minerIndex;
        SpecoAmount = specoAmount;
    }
}

[Serializable, NetSerializable]
public sealed class ToggleAutoBuyMinerMessage : BoundUserInterfaceMessage
{
    public int MinerIndex { get; }
    public bool Enabled { get; }
    public ToggleAutoBuyMinerMessage(int minerIndex, bool enabled)
    {
        MinerIndex = minerIndex;
        Enabled = enabled;
    }
}
