using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Audio;
using Content.Shared.DoAfter;

namespace Content.Shared.BloodCult;

#region DoAfters

[Serializable, NetSerializable] public sealed partial class DrawRuneDoAfterEvent : SimpleDoAfterEvent
{
	[NonSerialized] public EntityUid CarverUid;
	[NonSerialized] public EntityUid Rune;
    [NonSerialized] public EntityCoordinates Coords;
	[NonSerialized] public string EntityId;
	[NonSerialized] public int BleedOnCarve;
	[NonSerialized] public SoundSpecifier CarveSound;

    public DrawRuneDoAfterEvent(EntityUid carverUid, EntityUid rune, EntityCoordinates coords, string entityId, int bleedOnCarve, SoundSpecifier carveSound)
    {
		CarverUid = carverUid;
        Rune = rune;
        Coords = coords;
		EntityId = entityId;
		BleedOnCarve = bleedOnCarve;
		CarveSound = carveSound;
    }
}

#endregion
