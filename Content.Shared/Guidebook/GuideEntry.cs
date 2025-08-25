// SPDX-FileCopyrightText: 2023 Hebi <spiritbreakz@gmail.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Guidebook;

[Prototype]
public sealed partial class GuideEntryPrototype : GuideEntry, IPrototype
{
    public string ID => Id;
}

[Virtual]
public class GuideEntry
{
    /// <summary>
    ///     The file containing the contents of this guide.
    /// </summary>
    [DataField(required: true)] public ResPath Text = default!;

    /// <summary>
    ///     The unique id for this guide.
    /// </summary>
    [IdDataField]
    public string Id = default!;

    /// <summary>
    ///     The name of this guide. This gets localized.
    /// </summary>
    [DataField(required: true)] public string Name = default!;

    /// <summary>
    ///     The "children" of this guide for when guides are shown in a tree / table of contents.
    /// </summary>
    [DataField]
    public List<ProtoId<GuideEntryPrototype>> Children = new();

    /// <summary>
    ///     Enable filtering of items.
    /// </summary>
    [DataField] public bool FilterEnabled = default!;

    [DataField] public bool RuleEntry;

    /// <summary>
    ///     Priority for sorting top-level guides when shown in a tree / table of contents.
    ///     If the guide is the child of some other guide, the order simply determined by the order of children in <see cref="Children"/>.
    /// </summary>
    [DataField] public int Priority = 0;
}
