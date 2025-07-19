// SPDX-FileCopyrightText: 2025 Lyndomen <49795619+Lyndomen@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Database;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Content.Shared._Funkystation.Records;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Records;

public static class RecordsSerialization
{
    private static int DeserializeInt(JsonElement e, string key, int def)
    {
        if (e.TryGetProperty(key, out var prop) && prop.TryGetInt32(out var v))
        {
            return v;
        }

        return def;
    }

    private static bool DeserializeBool(JsonElement e, string key, bool def)
    {
        if (!e.TryGetProperty(key, out var v))
            return def;

        return v.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => def,
        };
    }

    [return: NotNullIfNotNull(nameof(def))]
    private static string? DeserializeString(JsonElement e, string key, string? def)
    {
        if (!e.TryGetProperty(key, out var v))
            return def;

        if (v.ValueKind == JsonValueKind.String)
            return v.GetString() ?? def;

        return def;
    }

    private static HashSet<ProtoId<MedicalInfoPrototype>> DeserializeSet(JsonElement e, string key)
    {
        var hashSet = new HashSet<ProtoId<MedicalInfoPrototype>>();

        if (!e.TryGetProperty(key, out var v))
            return [];

        if (v.ValueKind != JsonValueKind.Array)
            return [];

        var enumerator = v.EnumerateArray();

        while (enumerator.MoveNext())
        {
            var id = enumerator.Current.GetProperty("Id").GetString();
            if (id is not null)
                hashSet.Add(id);
        }

        return hashSet;

    }

    /// <summary>
    /// We need to manually deserialize CharacterRecords because the easy JSON deserializer does not
    /// do exactly what we want. More specifically, we need to more robustly handle missing and extra fields
    /// <br />
    /// <br />
    /// Missing fields are filled in with their default value, extra fields are simply ignored
    /// </summary>
    public static PlayerProvidedCharacterRecords Deserialize(JsonDocument json)
    {
        var e = json.RootElement;
        var def = PlayerProvidedCharacterRecords.DefaultRecords();
        return new PlayerProvidedCharacterRecords(
            height: DeserializeInt(e, nameof(def.Height), def.Height),
            weight: DeserializeInt(e, nameof(def.Weight), def.Weight),
            hasWorkAuthorization: DeserializeBool(e, nameof(def.HasWorkAuthorization), def.HasWorkAuthorization),
            identifyingFeatures: DeserializeString(e, nameof(def.IdentifyingFeatures), def.IdentifyingFeatures),
            hasInsurance: DeserializeBool(e, nameof(def.HasInsurance), def.HasInsurance),
            insuranceProvider: DeserializeInt(e, nameof(def.InsuranceProvider), def.InsuranceProvider),
            insuranceType: DeserializeInt(e, nameof(def.InsuranceType), def.InsuranceType),
            medicalInfo: DeserializeSet(e, nameof(def.MedicalInfo)),
            bloodType: DeserializeInt(e, nameof(def.BloodType), def.BloodType),
            postmortemInstructions: DeserializeString(e, nameof(def.PostmortemInstructions), def.PostmortemInstructions));
    }
}
