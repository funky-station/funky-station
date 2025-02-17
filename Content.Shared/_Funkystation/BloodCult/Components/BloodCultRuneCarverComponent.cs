using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.BloodCult.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultRuneCarverComponent : Component
{
	/// <summary>
    ///     The entity to spawn (e.g. animation) while carving.
    /// </summary>
    [DataField] public string InProgress = "Flash";

	/// <summary>
    ///     The entity to spawn when used on self.
    /// </summary>
    [DataField] public string Rune = "ExplosionActivateRune";

	/// <summary>
    ///     Blood damage to apply to self when used to carve a rune.
    /// </summary>
    [DataField] public int BleedOnCarve = 5;

	/// <summary>
    ///     Time in seconds needed to carve a rune.
    /// </summary>
    [DataField] public float TimeToCarve = 6f;

	/// <summary>
    ///     Sound that plays when used to carve a rune.
    /// </summary>
    [DataField] public SoundSpecifier CarveSound = new SoundCollectionSpecifier("gib");
}
