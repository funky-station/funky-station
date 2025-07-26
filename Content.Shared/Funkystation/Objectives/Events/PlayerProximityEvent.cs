using Content.Shared.Mind;

namespace Content.Shared.Objectives.Events;

[ByRefEvent]
public record struct PlayerProximityEvent(EntityUid Ent, TimeSpan ComponentUpdateTimeInterval);
