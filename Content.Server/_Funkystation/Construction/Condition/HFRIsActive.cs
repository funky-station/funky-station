using Content.Server._Funkystation.Atmos.Components;
using Content.Shared.Construction;
using JetBrains.Annotations;
using Content.Shared.Examine;

namespace Content.Server.Construction.Conditions;

[UsedImplicitly]
[DataDefinition]
public sealed partial class HFRIsActive : IGraphCondition
{
    public bool Condition(EntityUid uid, IEntityManager entityManager)
    {
        // Check each HFR component and return its IsActive property
        if (entityManager.TryGetComponent<HFRCornerComponent>(uid, out var corner) && corner.IsActive)
            return false;
        if (entityManager.TryGetComponent<HFRCoreComponent>(uid, out var core) && (core.IsActive || core.PowerLevel > 0))
            return false;
        if (entityManager.TryGetComponent<HFRFuelInputComponent>(uid, out var fuelInput) && fuelInput.IsActive)
            return false;
        if (entityManager.TryGetComponent<HFRModeratorInputComponent>(uid, out var moderatorInput) && moderatorInput.IsActive)
            return false;
        if (entityManager.TryGetComponent<HFRWasteOutputComponent>(uid, out var wasteOutput) && wasteOutput.IsActive)
            return false;
        if (entityManager.TryGetComponent<HFRConsoleComponent>(uid, out var console) && console.IsActive)
            return false;

        return true;
    }

    public bool DoExamine(ExaminedEvent args)
    {
        return false;
    }

    public IEnumerable<ConstructionGuideEntry> GenerateGuideEntry()
    {
        yield return new ConstructionGuideEntry();
    }
}