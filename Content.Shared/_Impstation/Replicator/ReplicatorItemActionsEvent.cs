using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Impstation.Replicator;

public sealed partial class ReplicatorOmnitoolActionEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class ReplicatorOmnitoolDoAfterEvent : SimpleDoAfterEvent
{
}

public sealed partial class ReplicatorWelderActionEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class ReplicatorWelderDoAfterEvent : SimpleDoAfterEvent
{
}


public sealed partial class ReplicatorArmActionEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class ReplicatorArmDoAfterEvent : SimpleDoAfterEvent
{
}

public sealed partial class ReplicatorAACActionEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class ReplicatorAACDoAfterEvent : SimpleDoAfterEvent
{
}
