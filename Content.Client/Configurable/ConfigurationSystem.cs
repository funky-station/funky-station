// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Configurable.UI;
using Content.Shared.Configurable;

namespace Content.Client.Configurable;

public sealed class ConfigurationSystem : SharedConfigurationSystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ConfigurationComponent, AfterAutoHandleStateEvent>(OnConfigurationState);
    }

    private void OnConfigurationState(Entity<ConfigurationComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (_uiSystem.TryGetOpenUi<ConfigurationBoundUserInterface>(ent.Owner,
                ConfigurationComponent.ConfigurationUiKey.Key,
                out var bui))
        {
            bui.Refresh(ent);
        }
    }
}
