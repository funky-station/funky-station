using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.BloodCult.Prototypes;

namespace Content.Shared.BloodCult.Components;

/// <summary>
/// Manufactured constructs that work for the blood cult.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodCultConstructComponent : Component
{
    /// <summary>
	///		Currently active spells.
	/// </summary>
	[DataField, AutoNetworkedField] public List<ProtoId<CultAbilityPrototype>> KnownSpells = new();

    /// <summary>
	///		Studies the veil.
	/// </summary>
	[DataField] public bool StudyingVeil = false;
}
