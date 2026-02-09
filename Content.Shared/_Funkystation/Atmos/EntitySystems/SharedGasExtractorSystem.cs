// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Funkystation.Atmos.Components;
using Content.Shared.Atmos.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Temperature;

namespace Content.Shared._Funkystation.Atmos.EntitySystems;

public abstract class SharedGasExtractorSystem : EntitySystem
{
    [Dependency] private readonly SharedAtmosphereSystem _sharedAtmosphereSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GasExtractorComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<GasExtractorComponent> ent, ref ExaminedEvent args)
    {
        var component = ent.Comp;

        using (args.PushGroup(nameof(GasExtractorComponent)))
        {
            args.PushMarkup(Loc.GetString("gas-extractor-extracts-text",
                ("gas", Loc.GetString(_sharedAtmosphereSystem.GetGas(component.SpawnGas).Name))));

            args.PushText(Loc.GetString("gas-extractor-amount-text",
                ("moles", $"{component.SpawnAmount:0.#}")));

            args.PushText(Loc.GetString("gas-extractor-temperature-text",
                ("tempK", $"{component.SpawnTemperature:0.#}"),
                ("tempC", $"{TemperatureHelpers.KelvinToCelsius(component.SpawnTemperature):0.#}")));

            if (component.MaxExternalAmount < float.PositiveInfinity)
            {
                args.PushText(Loc.GetString("gas-extractor-moles-cutoff-text",
                    ("moles", $"{component.MaxExternalAmount:0.#}")));
            }

            if (component.MaxExternalPressure < float.PositiveInfinity)
            {
                args.PushText(Loc.GetString("gas-extractor-pressure-cutoff-text",
                    ("pressure", $"{component.MaxExternalPressure:0.#}")));
            }

            args.AddMarkup(component.ExtractorState switch
            {
                GasExtractorState.Disabled => Loc.GetString("gas-extractor-state-disabled-text"),
                GasExtractorState.Idle => Loc.GetString("gas-extractor-state-idle-text"),
                GasExtractorState.Working => Loc.GetString("gas-extractor-state-working-text"),
                // C# pattern matching is not exhaustive for enums
                _ => throw new IndexOutOfRangeException(nameof(component.ExtractorState)),
            });
        }
    }
}
