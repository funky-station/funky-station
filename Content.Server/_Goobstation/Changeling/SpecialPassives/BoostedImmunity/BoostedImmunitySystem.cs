using Content.Goobstation.Shared.Medical;
using Content.Goobstation.Shared.SpecialPassives.BoostedImmunity.Components;
using Content.Server.Body.Systems;
using Content.Shared.Body.Part;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.SpecialPassives.BoostedImmunity;

public sealed class BoostedImmunitySystem : SharedBoostedImmunitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly BodySystem _body = default!;

    public readonly ProtoId<DisabilityListPrototype> DisabilityProto = "AllDisabilities";
    protected override void RemoveDisabilities(Entity<BoostedImmunityComponent> ent)
    {
        if (!_protoManager.TryIndex(DisabilityProto, out var disabilityList))
            return;

        EntityManager.RemoveComponents(ent, disabilityList.Components);
    }
}
