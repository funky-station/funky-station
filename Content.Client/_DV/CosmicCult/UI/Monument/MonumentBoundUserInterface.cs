// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult.Prototypes;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.CosmicCult.UI.Monument;

[UsedImplicitly]
public sealed class MonumentBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private MonumentMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<MonumentMenu>();

        _menu.OnSelectGlyphButtonPressed += protoId => SendMessage(new GlyphSelectedMessage(protoId));
        _menu.OnRemoveGlyphButtonPressed += () => SendMessage(new GlyphRemovedMessage());

        _menu.OnGainButtonPressed += OnInfluenceSelected;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not MonumentBuiState buiState)
            return;

        _menu?.UpdateState(buiState);
    }

    private void OnInfluenceSelected(ProtoId<InfluencePrototype> selectedInfluence)
    {
        SendMessage(new InfluenceSelectedMessage(selectedInfluence));
    }
}
