using Content.Server.GameTicking.Rules;
namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Component for the BloodCultRuleSystem that stores info about winning/losing, player counts required
///	for stuff, and other round-wide stuff.
/// </summary>
[RegisterComponent, Access(typeof(BloodCultRuleSystem))]
public sealed partial class BloodCultRuleComponent : Component
{
	
}
