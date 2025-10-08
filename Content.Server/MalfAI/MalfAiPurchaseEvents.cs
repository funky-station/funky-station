using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Server.MalfAI;

/// <summary>
/// Malf AI purchase events: raised by the store when specific upgrades are bought.
/// </summary>


[Serializable, DataDefinition]
public sealed partial class MalfAiSyndicateKeysUnlockedEvent : EntityEventArgs
{
}

[Serializable, DataDefinition]
public sealed partial class MalfAiCameraUpgradeUnlockedEvent : EntityEventArgs
{
}

[Serializable, DataDefinition]
public sealed partial class MalfAiCameraMicrophonesUnlockedEvent : EntityEventArgs
{
}
