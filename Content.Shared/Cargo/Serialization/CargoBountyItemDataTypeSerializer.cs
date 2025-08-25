// SPDX-FileCopyrightText: 2025 Gansu <68031780+GansuLalan@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Cargo.Prototypes;
using Content.Shared.Construction.Steps;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;

namespace Content.Shared.Cargo.Serialization;


[TypeSerializer]
public sealed class CargoBountyItemDataTypeSerializer : ITypeReader<CargoBountyItemData, MappingDataNode>
{
    private Type? GetType(MappingDataNode node)
    {
        if (node.Has("whitelist"))
        {
            return typeof(CargoObjectBountyItemData);
        }

        if (node.Has("reagent"))
        {
            return typeof(CargoReagentBountyItemData);
        }

        return null;
    }
    public CargoBountyItemData Read(ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<CargoBountyItemData>? instanceProvider = null)
    {
        var type = GetType(node) ??
            throw new ArgumentException(
                "Tried to convert invalid YAML node mapping to ConstructionGraphStep!");
        return (CargoBountyItemData)serializationManager.Read(type, node, hookCtx, context)!;
    }
    public ValidationNode Validate(ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        var type = GetType(node);
        if (type == null)
            return new ErrorNode(node, "No construction graph step type found.");
        return serializationManager.ValidateNode(type, node, context);
    }
}

