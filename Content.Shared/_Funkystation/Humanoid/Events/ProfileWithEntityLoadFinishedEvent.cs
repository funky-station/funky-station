using Content.Shared.Preferences;

namespace Content.Shared._Funkystation.Humanoid.Events;

public record struct ProfileWithEntityLoadFinishedEvent(EntityUid Uid, HumanoidCharacterProfile Profile);