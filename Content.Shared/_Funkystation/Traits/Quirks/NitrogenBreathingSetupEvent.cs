using JetBrains.Annotations;

namespace Content.Shared._Funkystation.Traits.Quirks;

/// <summary>
///
/// </summary>
[PublicAPI]
public sealed class NitrogenBreathingSetup : EntityEventArgs
{
    public EntityUid Uid { get; }
    public NitrogenBreathingSetup(EntityUid uid) { Uid = uid; }
}
