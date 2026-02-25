using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Content.Shared.Mind;

namespace Content.Shared._Funkystation.Manifest;

public sealed class DeathInformationMessage : NetMessage
{
    public string Description = string.Empty;
    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Description = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Description);
    }

    public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.ReliableUnordered;
}

[Serializable, NetSerializable]
public sealed class DeathInfoOpenMessage : EntityEventArgs;
