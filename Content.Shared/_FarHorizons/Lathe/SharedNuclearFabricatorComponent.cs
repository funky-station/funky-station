using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Lathe;

[Serializable, NetSerializable]
public enum NuclearFabricatorUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class NuclearFabricatorBuiState : BoundUserInterfaceState
{

}