// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Stylesheets;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Temperature;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using System.Linq;

namespace Content.Client.Atmos.Consoles;

[GenerateTypedNameReferences]
public sealed partial class AtmosMonitoringEntryContainer : BoxContainer
{
    public AtmosMonitoringConsoleEntry Data;

    private readonly IEntityManager _entManager;
    private readonly IResourceCache _cache;

    public AtmosMonitoringEntryContainer(AtmosMonitoringConsoleEntry data)
    {
        RobustXamlLoader.Load(this);
        _entManager = IoCManager.Resolve<IEntityManager>();
        _cache = IoCManager.Resolve<IResourceCache>();

        Data = data;

        // Modulate colored stripe
        NetworkColorStripe.Modulate = data.Color;

        // Load fonts
        var headerFont = new VectorFont(_cache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Bold.ttf"), 11);
        var normalFont = new VectorFont(_cache.GetResource<FontResource>("/Fonts/NotoSansDisplay/NotoSansDisplay-Regular.ttf"), 11);

        // Set fonts
        TemperatureHeaderLabel.FontOverride = headerFont;
        PressureHeaderLabel.FontOverride = headerFont;
        TotalMolHeaderLabel.FontOverride = headerFont;
        GasesHeaderLabel.FontOverride = headerFont;

        TemperatureLabel.FontOverride = normalFont;
        PressureLabel.FontOverride = normalFont;
        TotalMolLabel.FontOverride = normalFont;

        NoDataLabel.FontOverride = headerFont;
    }

    public void UpdateEntry(AtmosMonitoringConsoleEntry updatedData, bool isFocus)
    {
        // Load fonts
        var normalFont = new VectorFont(_cache.GetResource<FontResource>("/Fonts/NotoSansDisplay/NotoSansDisplay-Regular.ttf"), 11);

        // Update name and values
        if (!string.IsNullOrEmpty(updatedData.Address))
            NetworkNameLabel.Text = Loc.GetString("atmos-alerts-window-alarm-label", ("name", updatedData.EntityName), ("address", updatedData.Address));

        else
            NetworkNameLabel.Text = Loc.GetString(updatedData.EntityName);

        Data = updatedData;

        // Modulate colored stripe
        NetworkColorStripe.Modulate = Data.Color;

        // Focus updates
        if (isFocus)
            SetAsFocus();
        else
            RemoveAsFocus();

        // Check if powered
        if (!updatedData.IsPowered)
        {
            MainDataContainer.Visible = false;
            NoDataLabel.Visible = true;

            return;
        }

        // Set container visibility
        MainDataContainer.Visible = true;
        NoDataLabel.Visible = false;

        // Update temperature
        var isNotVacuum = updatedData.TotalMolData > 1e-6f;
        var tempK = (FixedPoint2)updatedData.TemperatureData;
        var tempC = (FixedPoint2)TemperatureHelpers.KelvinToCelsius(tempK.Float());

        TemperatureLabel.Text = isNotVacuum ?
            Loc.GetString("atmos-alerts-window-temperature-value", ("valueInC", tempC), ("valueInK", tempK)) :
            Loc.GetString("atmos-alerts-window-invalid-value");

        TemperatureLabel.FontColorOverride = isNotVacuum ? Color.DarkGray : StyleNano.DisabledFore;

        // Update pressure
        PressureLabel.Text = Loc.GetString("atmos-alerts-window-pressure-value", ("value", (FixedPoint2)updatedData.PressureData));
        PressureLabel.FontColorOverride = isNotVacuum ? Color.DarkGray : StyleNano.DisabledFore;

        // Update total mol
        TotalMolLabel.Text = Loc.GetString("atmos-alerts-window-total-mol-value", ("value", (FixedPoint2)updatedData.TotalMolData));
        TotalMolLabel.FontColorOverride = isNotVacuum ? Color.DarkGray : StyleNano.DisabledFore;

        // Update other present gases
        GasGridContainer.RemoveAllChildren();

        if (updatedData.GasData.Count() == 0)
        {
            // No gases
            var gasLabel = new Label()
            {
                Text = Loc.GetString("atmos-alerts-window-other-gases-value-nil"),
                FontOverride = normalFont,
                FontColorOverride = StyleNano.DisabledFore,
                HorizontalAlignment = HAlignment.Center,
                VerticalAlignment = VAlignment.Center,
                HorizontalExpand = true,
                Margin = new Thickness(0, 2, 0, 0),
                SetHeight = 24f,
            };

            GasGridContainer.AddChild(gasLabel);
        }

        else
        {
            // Add an entry for each gas
            foreach (var (gas, percent) in updatedData.GasData)
            {
                var gasPercent = (FixedPoint2)0f;
                gasPercent = percent * 100f;

                var gasAbbreviation = Atmospherics.GasAbbreviations.GetValueOrDefault(gas, Loc.GetString("gas-unknown-abbreviation"));

                var gasLabel = new Label()
                {
                    Text = Loc.GetString("atmos-alerts-window-other-gases-value", ("shorthand", gasAbbreviation), ("value", gasPercent)),
                    FontOverride = normalFont,
                    HorizontalAlignment = HAlignment.Center,
                    VerticalAlignment = VAlignment.Center,
                    HorizontalExpand = true,
                    Margin = new Thickness(0, 2, 0, 0),
                    SetHeight = 24f,
                };

                GasGridContainer.AddChild(gasLabel);
            }
        }
    }

    public void SetAsFocus()
    {
        FocusButton.AddStyleClass(StyleNano.StyleClassButtonColorGreen);
        ArrowTexture.TexturePath = "/Textures/Interface/Nano/inverted_triangle.svg.png";
        FocusContainer.Visible = true;
    }

    public void RemoveAsFocus()
    {
        FocusButton.RemoveStyleClass(StyleNano.StyleClassButtonColorGreen);
        ArrowTexture.TexturePath = "/Textures/Interface/Nano/triangle_right.png";
        FocusContainer.Visible = false;
    }
}
