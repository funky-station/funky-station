using Content.Shared.Destructible.Thresholds;
using Content.Shared.Procedural;
using Content.Shared.Procedural.DungeonLayers;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
// borrowed, likely overlap
using System.Linq;
using Content.Shared.CCVar;
using Content.Shared.Dataset;
using Content.Shared.Procedural;
using Content.Shared.Procedural.Loot;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.Salvage.Expeditions;
using Content.Shared.Salvage.Expeditions.Modifiers;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Funkystation.Mining;

public abstract partial class SharedMiningSystem : EntitySystem
{
    [Dependency] protected readonly IConfigurationManager CfgManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    private readonly Dictionary<IMiningMagnetOffering, float> _offeringWeights = new()
    {
        { new AsteroidOffering(), 4.5f },
    };

    private readonly List<ProtoId<DungeonConfigPrototype>> _asteroidConfigs = new()
    {
        "BlobAsteroid",
        "ClusterAsteroid",
        "SpindlyAsteroid",
        "SwissCheeseAsteroid"
    };

    private readonly ProtoId<WeightedRandomPrototype> _asteroidOreWeights = "AsteroidOre";

    private readonly MinMax _asteroidOreCount = new(5, 7);

    public IMiningMagnetOffering GetMiningOffering(int seed)
    {
        var rand = new System.Random(seed);

        var type = SharedRandomExtensions.Pick(_offeringWeights, rand);
        switch (type)
        {
            case AsteroidOffering:
                var configId = _asteroidConfigs[rand.Next(_asteroidConfigs.Count)];
                var configProto =_proto.Index(configId);
                var layers = new Dictionary<string, int>();

                var data = new DungeonData();
                data.Apply(configProto.Data);

                var config = new DungeonConfig
                {
                    Data = data,
                    Layers = new(configProto.Layers),
                    MaxCount = configProto.MaxCount,
                    MaxOffset = configProto.MaxOffset,
                    MinCount = configProto.MinCount,
                    MinOffset = configProto.MinOffset,
                    ReserveTiles = configProto.ReserveTiles
                };

                var count = _asteroidOreCount.Next(rand);
                var weightedProto = _proto.Index(_asteroidOreWeights);
                for (var i = 0; i < count; i++)
                {
                    var ore = weightedProto.Pick(rand);
                    config.Layers.Add(_proto.Index<OreDunGenPrototype>(ore));

                    var layerCount = layers.GetOrNew(ore);
                    layerCount++;
                    layers[ore] = layerCount;
                }

                return new AsteroidOffering
                {
                    Id = configId,
                    DungeonConfig = config,
                    MarkerLayers = layers,
                };
            default:
                throw new NotImplementedException($"Salvage type {type} not implemented!");
        }
    }
}
