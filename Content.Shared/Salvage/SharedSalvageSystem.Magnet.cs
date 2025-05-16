using Content.Shared.Destructible.Thresholds;
using Content.Shared.Procedural;
using Content.Shared.Procedural.DungeonLayers;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.Salvage.Magnet;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared.Salvage;

public abstract partial class SharedSalvageSystem
{
    private readonly List<SalvageMapPrototype> _salvageMaps = new();

    private readonly Dictionary<ISalvageMagnetOffering, float> _offeringWeights = new()
    {
        { new DebrisOffering(), 3.5f },
        { new SalvageOffering(), 2.0f },
    };

    private readonly List<ProtoId<DungeonConfigPrototype>> _debrisConfigs = new()
    {
        "ChunkDebris"
    };

    public ISalvageMagnetOffering GetSalvageOffering(int seed)
    {
        var rand = new System.Random(seed);

        var type = SharedRandomExtensions.Pick(_offeringWeights, rand);
        switch (type)
        {
            case DebrisOffering:
                var id = rand.Pick(_debrisConfigs);
                return new DebrisOffering
                {
                    Id = id
                };
            case SalvageOffering:
                // Salvage map seed
                _salvageMaps.Clear();
                _salvageMaps.AddRange(_proto.EnumeratePrototypes<SalvageMapPrototype>());
                _salvageMaps.Sort((x, y) => string.Compare(x.ID, y.ID, StringComparison.Ordinal));
                var mapIndex = rand.Next(_salvageMaps.Count);
                var map = _salvageMaps[mapIndex];

                return new SalvageOffering
                {
                    SalvageMap = map,
                };
            default:
                throw new NotImplementedException($"Salvage type {type} not implemented!");
        }
    }
}
