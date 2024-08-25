using Robust.Shared.GameStates;

namespace Content.Shared.Obsessed;

[RegisterComponent, NetworkedComponent]
public sealed partial class ObsessedComponent : Component
{
    public float HugAmount = 0f;
    public EntityUid TargetUid = EntityUid.Invalid;
    public string TargetName = "";
}
