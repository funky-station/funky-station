using Content.Shared.Body.Organ;
using Content.Shared.Body.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Heretic;

[ByRefEvent]
public record struct GetBodyOrganOverrideEvent<T>(Entity<T, OrganComponent>? Organ) where T : IComponent;
