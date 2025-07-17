// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Light.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Utility;

namespace Content.Client.Silicons.StationAi;

public sealed partial class StationAiSystem
{
    // Used for surveillance camera lights

    private void InitializePowerToggle()
    {
        SubscribeLocalEvent<ItemTogglePointLightComponent, GetStationAiRadialEvent>(OnLightGetRadial);
    }

    private void OnLightGetRadial(Entity<ItemTogglePointLightComponent> ent, ref GetStationAiRadialEvent args)
    {
        if (!TryComp(ent.Owner, out ItemToggleComponent? toggle))
            return;

        args.Actions.Add(new StationAiRadial()
        {
            Tooltip = Loc.GetString("toggle-light"),
            Sprite = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/light.svg.192dpi.png")),
            Event = new StationAiLightEvent()
            {
                Enabled = !toggle.Activated
            }
        });
    }
}
