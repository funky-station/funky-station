using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Utility;

namespace Content.Server.Maps;

/// <summary>
///     Performs basic map migration operations by listening for engine <see cref="MapLoaderSystem"/> events.
/// </summary>
public sealed class MapMigrationSystem : EntitySystem
{
#pragma warning disable CS0414
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
#pragma warning restore CS0414
    [Dependency] private readonly IResourceManager _resMan = default!;

    private List<string> _migrationFiles = [ "/migration.yml", "/funky_migration.yml" ];

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BeforeEntityReadEvent>(OnBeforeReadEvent);

#if DEBUG
        var mappings = TryReadFiles();

        // Verify that all of the entries map to valid entity prototypes.
        foreach (var node in mappings.Children.Values)
        {
            var newId = ((ValueDataNode) node).Value;
            if (!string.IsNullOrEmpty(newId) && newId != "null")
                DebugTools.Assert(_protoMan.HasIndex<EntityPrototype>(newId), $"{newId} is not an entity prototype.");
        }
#endif
    }

    private MappingDataNode TryReadFiles()
    {
        var mappings = new MappingDataNode();

        foreach (var path in _migrationFiles.Select(file => new ResPath(file)))
        {
            if (!_resMan.TryContentFileRead(path, out var stream))
                continue;
            
            using var reader = new StreamReader(stream, EncodingHelpers.UTF8);
            var document = DataNodeParser.ParseYamlStream(reader).FirstOrDefault();
            
            if (document == null) 
                continue;
            
            var docRoot = (MappingDataNode) document.Root;
            
            foreach (var node in docRoot.Children)
            {
                if (mappings.TryAdd(node.Key, node.Value));
            }
        }

        return mappings;
    }

    private void OnBeforeReadEvent(BeforeEntityReadEvent ev)
    {
        var mappings = TryReadFiles();

        foreach (var (key, value) in mappings)
        {
            if (value is not ValueDataNode valueNode)
                continue;

            if (string.IsNullOrWhiteSpace(valueNode.Value) || valueNode.Value == "null")
                ev.DeletedPrototypes.Add(key);
            else
                ev.RenamedPrototypes.Add(key, valueNode.Value);
        }
    }
}
