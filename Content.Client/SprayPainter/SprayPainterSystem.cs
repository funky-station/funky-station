// SPDX-FileCopyrightText: 2023 c4llv07e <38111072+c4llv07e@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.SprayPainter;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Utility;
using System.Linq;
using Robust.Shared.Graphics;

namespace Content.Client.SprayPainter;

public sealed class SprayPainterSystem : SharedSprayPainterSystem
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    public List<SprayPainterEntry> Entries { get; private set; } = new();

    protected override void CacheStyles()
    {
        base.CacheStyles();

        Entries.Clear();
        foreach (var style in Styles)
        {
            var name = style.Name;
            string? iconPath = Groups
              .FindAll(x => x.StylePaths.ContainsKey(name))?
              .MaxBy(x => x.IconPriority)?.StylePaths[name];
            if (iconPath == null)
            {
                Entries.Add(new SprayPainterEntry(name, null));
                continue;
            }

            RSIResource doorRsi = _resourceCache.GetResource<RSIResource>(SpriteSpecifierSerializer.TextureRoot / new ResPath(iconPath));
            if (!doorRsi.RSI.TryGetState("closed", out var icon))
            {
                Entries.Add(new SprayPainterEntry(name, null));
                continue;
            }

            Entries.Add(new SprayPainterEntry(name, icon.Frame0));
        }
    }
}

public sealed class SprayPainterEntry
{
    public string Name;
    public Texture? Icon;

    public SprayPainterEntry(string name, Texture? icon)
    {
        Name = name;
        Icon = icon;
    }
}
