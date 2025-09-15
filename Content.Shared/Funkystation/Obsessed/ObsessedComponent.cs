﻿using System.Collections;
using Robust.Shared.GameStates;

namespace Content.Shared.Obsessed;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ObsessedComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float HugAmount = 0f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntityUid TargetUid = EntityUid.Invalid;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public string TargetName = "";

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public TimeSpan TimeSpent;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<string, bool> CompletedObjectives = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan UpdateTimeInterval = TimeSpan.FromSeconds(1);

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;
}
