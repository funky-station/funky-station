using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Lathe;

[NetSerializable, Serializable]
public enum NuclearFabricatorUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class NuclearFabricatorBuiState : BoundUserInterfaceState
{

}