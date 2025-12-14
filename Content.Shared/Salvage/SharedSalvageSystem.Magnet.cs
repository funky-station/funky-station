// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

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
    private readonly List<RuinMapPrototype> _ruinMaps = new();

    private readonly Dictionary<ISalvageMagnetOffering, float> _offeringWeights = new()
    {
        { new AsteroidOffering(), 4.5f },
        { new RuinOffering(), 3.5f },
        { new SalvageOffering(), 2.0f }
        //{ new DebrisOffering(), 3.5f } // disabled due to ruins replacing debris
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

    private readonly List<ProtoId<DungeonConfigPrototype>> _debrisConfigs = new()
    {
        "ChunkDebris"
    };

    /// <summary>
    /// Generates a Nanotrasen-style station name for a ruin map.
    /// Format: NT[MapCode]-[SuffixCode]-[Number]
    /// Example: NTVG-LV-427
    /// </summary>
    private string GenerateRuinStationName(RuinMapPrototype ruinMap, System.Random rand)
    {
        // Extract a 2-letter code from the map name/ID
        var mapCode = ruinMap.ID.Length >= 2 ? ruinMap.ID.Substring(0, 2).ToUpperInvariant() : "XX";
        
        // Nanotrasen suffix codes (same as NanotrasenNameGenerator)
        var suffixCodes = new[] { "LV", "NX", "EV", "QT", "PR" };
        var suffix = suffixCodes[rand.Next(suffixCodes.Length)];
        
        // Generate a random 3-digit number
        var number = rand.Next(0, 999);
        
        return $"NT{mapCode}-{suffix}-{number:D3}";
    }

    public ISalvageMagnetOffering GetSalvageOffering(int seed)
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
            case RuinOffering:
                // Ruin map seed
                _ruinMaps.Clear();
                _ruinMaps.AddRange(_proto.EnumeratePrototypes<RuinMapPrototype>());
                _ruinMaps.Sort((x, y) => string.Compare(x.ID, y.ID, StringComparison.Ordinal));
                if (_ruinMaps.Count == 0)
                {
                    // Fallback if no ruin maps are defined
                    throw new InvalidOperationException("No ruin map prototypes are defined for ruin offerings.");
                }
                var ruinMapIndex = rand.Next(_ruinMaps.Count);
                var ruinMap = _ruinMaps[ruinMapIndex];
                
                // Generate a station name for the ruin
                var stationName = GenerateRuinStationName(ruinMap, rand);

                return new RuinOffering
                {
                    RuinMap = ruinMap,
                    StationName = stationName,
                };
            default:
                throw new NotImplementedException($"Salvage type {type} not implemented!");
        }
    }
}
