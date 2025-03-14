using Robust.Shared.GameStates;

namespace Content.Shared.BloodCult.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultRuneComponent : Component
{
	/// <summary>
    ///     The animation to play when the rune is being drawn.
    /// </summary>
	[DataField] public string InProgress = "ReviveDrawingRune";
}
