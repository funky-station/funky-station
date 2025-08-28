// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.CosmicCult;

[Serializable, NetSerializable]
public enum MonumentKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class MonumentBuiState : BoundUserInterfaceState
{
    public int CurrentProgress;
    public int TargetProgress;
    public ProtoId<GlyphPrototype> SelectedGlyph;
    public HashSet<ProtoId<GlyphPrototype>> UnlockedGlyphs;

    public MonumentBuiState(int currentProgress, int targetProgress, int progressOffset, ProtoId<GlyphPrototype> selectedGlyph, HashSet<ProtoId<GlyphPrototype>> unlockedGlyphs)
    {
        CurrentProgress = currentProgress - progressOffset;
        TargetProgress = targetProgress - progressOffset;
        SelectedGlyph = selectedGlyph;
        UnlockedGlyphs = unlockedGlyphs;
    }

    public MonumentBuiState(MonumentComponent comp)
    {
        CurrentProgress = comp.CurrentProgress - comp.ProgressOffset;
        TargetProgress = comp.TargetProgress - comp.ProgressOffset;
        SelectedGlyph = comp.SelectedGlyph;
        UnlockedGlyphs = comp.UnlockedGlyphs;
    }
}
