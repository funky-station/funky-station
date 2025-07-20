// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Cargo.Systems;
using Content.Shared.Xenoarchaeology.Artifact;
using Content.Shared.Xenoarchaeology.Artifact.Components;

namespace Content.Server.Xenoarchaeology.Artifact;

/// <inheritdoc cref="SharedXenoArtifactSystem"/>
public sealed partial class XenoArtifactSystem : SharedXenoArtifactSystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenoArtifactComponent, MapInitEvent>(OnArtifactMapInit);
        SubscribeLocalEvent<XenoArtifactComponent, PriceCalculationEvent>(OnCalculatePrice);
    }

    private void OnArtifactMapInit(Entity<XenoArtifactComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.IsGenerationRequired)
            GenerateArtifactStructure(ent);
    }

    private void OnCalculatePrice(Entity<XenoArtifactComponent> ent, ref PriceCalculationEvent args)
    {
        foreach (var node in GetAllNodes(ent))
        {
            if (node.Comp.Locked)
                continue;

            args.Price += node.Comp.ResearchValue * ent.Comp.PriceMultiplier;
        }
    }
}
