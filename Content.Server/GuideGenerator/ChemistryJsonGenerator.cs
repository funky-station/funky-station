// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Sam Weaver <weaversam8@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server.GuideGenerator;

public sealed class ChemistryJsonGenerator
{
    public static void PublishJson(StreamWriter file)
    {
        var prototype = IoCManager.Resolve<IPrototypeManager>();
        var prototypes =
            prototype
                .EnumeratePrototypes<ReagentPrototype>()
                .Where(x => !x.Abstract)
                .Select(x => new ReagentEntry(x))
                .ToDictionary(x => x.Id, x => x);

        var reactions =
            prototype
                .EnumeratePrototypes<ReactionPrototype>()
                .Where(x => x.Products.Count != 0);

        foreach (var reaction in reactions)
        {
            foreach (var product in reaction.Products.Keys)
            {
                prototypes[product].Recipes.Add(reaction.ID);
            }
        }

        var serializeOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new UniversalJsonConverter<EntityEffect>(),
                new UniversalJsonConverter<EntityEffectCondition>(),
                new UniversalJsonConverter<ReagentEffectsEntry>(),
                new UniversalJsonConverter<DamageSpecifier>(),
                new FixedPointJsonConverter()
            }
        };

        file.Write(JsonSerializer.Serialize(prototypes, serializeOptions));
    }

    public sealed class FixedPointJsonConverter : JsonConverter<FixedPoint2>
    {
        public override void Write(Utf8JsonWriter writer, FixedPoint2 value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Float());
        }

        public override FixedPoint2 Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions options)
        {
            // Throwing a NotSupportedException here allows the error
            // message to provide path information.
            throw new NotSupportedException();
        }
    }
}
