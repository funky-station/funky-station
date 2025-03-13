using Content.Server.GameTicking.Rules;

namespace Content.Server.BloodCult.Components;

/// <summary>
/// Given to cult structures with buyable entities.
/// </summary>
[RegisterComponent]
public sealed partial class CultStoreComponent : Component
{
	/// <summary>
	///	The maximum energy allowed in this store.
	/// </summary>
	[DataField] public int MaximumEnergy = 50;

	/// <summary>
	///	The time in seconds between point recharges for the store.
	/// </summary>
	[DataField] public float TimeUntilRecharge = 5.0f;

	/// <summary>
	///	Current accumulated time.
	/// </summary>
	[DataField] public float TimeElapsed = 0.0f;
}
