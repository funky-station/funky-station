// SPDX-FileCopyrightText: 2026 beck-thompson <beck314159@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Starlight.DocumentManager;
using Content.Shared.Paper;

namespace Content.Shared._Starlight.Paper;

public sealed class TextFilePaperContentSystem : EntitySystem
{
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly PreWrittenDocumentManager _documentManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TextFilePaperContentComponent, MapInitEvent>(OnTextFilePaperContentComponentInit);
    }

    private void OnTextFilePaperContentComponentInit(Entity<TextFilePaperContentComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<PaperComponent>(ent, out var paperComp))
            return;

        if (!_documentManager.TryGetDocumentContents(ent.Comp.FileName, out var contents))
            return;

        _paper.SetContent((ent, paperComp), contents);

        RemCompDeferred(ent, ent.Comp);
    }
}
