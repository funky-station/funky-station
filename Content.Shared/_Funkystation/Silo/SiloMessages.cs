using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Silo;

[Serializable, NetSerializable]
public sealed class SiloUpdateState : BoundUserInterfaceState
{

}

/// <summary>
///     Sent to the server to sync material storage and the recipe queue.
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheSyncRequestMessage : BoundUserInterfaceMessage
{

}

[NetSerializable, Serializable]
public enum SiloUiKey
{
    Key,
}
