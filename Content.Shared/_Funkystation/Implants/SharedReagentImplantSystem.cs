using Content.Shared.Actions;

namespace Content.Shared._Funkystation.Implants;

public abstract class SharedReagentImplantSystem : EntitySystem { }

public sealed partial class UseReagentImplantEvent : InstantActionEvent { }
