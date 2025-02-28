using System.Text.Json.Serialization;
using Robust.Shared.Prototypes;
using Content.Shared.EntityEffects;
using Content.Shared.BloodCult;

namespace Content.Server.EntityEffects.Effects;

//[UsedImplicitly]
public sealed partial class DeCultify : EntityEffect
{
	/// <summary>
	/// Decultification to apply every cycle.
	/// </summary>
	[DataField(required: true)]
	[JsonPropertyName("amount")]
	public float Amount = default!;

	protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
	{
		return "In large quantities, can free a person's mind from servitude to an eldritch entity.";
	}

	public override void Effect(EntityEffectBaseArgs args)
	{
		if (!args.EntityManager.TryGetComponent(args.TargetEntity, out BloodCultistComponent? bloodCultist))
			return;

		var scale = 1.0f;

		if (args is EntityEffectReagentArgs reagentArgs)
		{
			scale = reagentArgs.Scale.Float();
		}

		bloodCultist.DeCultification += Amount * scale;
	}
}
