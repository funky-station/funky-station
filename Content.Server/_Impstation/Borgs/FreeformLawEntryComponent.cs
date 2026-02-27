// SPDX-FileCopyrightText: 2026 beck <163376292+widgetbeck@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Utility;

namespace Content.Server._Impstation.Borgs.FreeformLaws;

/// <summary>
/// Adds a verb to allow custom law entry on SiliconLawProviders. Should probably never be added to anything that isn't a lawboard.
/// </summary>
[RegisterComponent]
public sealed partial class FreeformLawEntryComponent : Component
{
    [DataField]
    public LocId VerbName = "silicon-law-ui-verb";

    [DataField]
    public SpriteSpecifier? VerbIcon = null;
}
