using Content.Server._Funkystation.Payouts.Components;
using Content.Shared.StationRecords;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Payouts;

public abstract class SharedScripSystem : EntitySystem
{
    [Dependency] private readonly SharedStationRecordsSystem _records = default!;
}

