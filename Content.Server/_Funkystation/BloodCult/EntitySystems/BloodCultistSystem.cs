using Content.Shared.BloodCult;

namespace Content.Server.BloodCult;

public sealed class BloodCultistSystem : SharedBloodCultistSystem
{
	public void UseReviveRune(EntityUid target, EntityUid? user, EntityUid? used)
	{
		var attempt = new ReviveRuneAttemptEvent(target, user, used);
		RaiseLocalEvent(target, attempt, true);
	}

	public void UseGhostifyRune(EntityUid target, EntityUid? user, EntityUid used)
	{
		var attempt = new GhostifyRuneEvent(target, user, used);
		RaiseLocalEvent(target, attempt, true);
	}

	public void UseSacrificeRune(EntityUid target, EntityUid user, EntityUid used, EntityUid[] otherCultists)
	{
		var attempt = new SacrificeRuneEvent(target, user, used, otherCultists);
		RaiseLocalEvent(user, attempt, true);
	}

	public void UseConvertRune(EntityUid target, EntityUid user, EntityUid used, EntityUid[] otherCultists)
	{
		var attempt = new ConvertRuneEvent(target, user, used, otherCultists);
		RaiseLocalEvent(user, attempt, true);
	}
}
