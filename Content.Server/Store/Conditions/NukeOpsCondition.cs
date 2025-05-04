using Content.Server.GameTicking.Rules;
using Content.Server.NukeOps;
using Content.Shared.NukeOps;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

public override bool Condition(ListingConditionArgs args)
{
// get reference to warops system
if (WarDeclaratorComponent.WarConditionStatus) return true;
else return false;
}
