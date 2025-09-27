// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PoTeletubby <ajcraigaz@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Paper;
using Content.Shared.StoryGen;

namespace Content.Server.Paper;

public sealed class PaperRandomStorySystem : EntitySystem
{
    [Dependency] private readonly StoryGeneratorSystem _storyGen = default!;
    [Dependency] private readonly PaperSystem _paper = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PaperRandomStoryComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<PaperRandomStoryComponent> paperStory, ref MapInitEvent ev)
    {
        if (!TryComp<PaperComponent>(paperStory, out var paper))
            return;

        if (!_storyGen.TryGenerateStoryFromTemplate(paperStory.Comp.Template, out var story))
            return;

        _paper.SetContent((paperStory.Owner, paper), story);
    }
}
