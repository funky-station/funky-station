using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared.BloodCult.EntityEffects;

/// <summary>
/// When blood is splashed on a juggernaut, creates corrupted blood puddles.
/// </summary>
[UsedImplicitly]
public sealed partial class JuggernautBloodCorruptionEffect
    : EntityEffectBase<JuggernautBloodCorruptionEffect>
{
    [DataField]
    public ProtoId<ReagentPrototype> CorruptedReagent = "SanguinePerniculate";
}
