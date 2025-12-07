// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Funkystation.Atmos.Prototypes
{
    [Prototype("crystallizerRecipe")]
    public sealed class CrystallizerRecipePrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField("name")]
        public string Name { get; private set; } = default!;

        [DataField("minimumTemperature")]
        public float MinimumTemperature { get; private set; }

        [DataField("maximumTemperature")]
        public float MaximumTemperature { get; private set; }

        [DataField("minimumRequirements")]
        public float[] MinimumRequirements { get; private set; } = default!;

        [DataField("energyRelease")]
        public float EnergyRelease { get; private set; }

        [DataField("products")]
        public Dictionary<string, int> Products { get; private set; } = new();

        [DataField("dangerous")]
        public bool Dangerous { get; private set; }
    }
}