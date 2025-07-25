// SPDX-FileCopyrightText: 2024 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.UserInterface.Controls;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;
using Content.Shared.MassMedia.Systems;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.MassMedia.Ui;

[GenerateTypedNameReferences]
public sealed partial class NewsWriterMenu : FancyWindow
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private TimeSpan? _nextPublish;

    public event Action<int>? DeleteButtonPressed;

    public event Action? CreateButtonPressed;

    public NewsWriterMenu()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        ContentsContainer.RectClipContent = false;

        // Customize scrollbar width and margin. This is not possible in xaml
        var scrollbar = ArticleListScrollbar.GetChild(1);
        scrollbar.SetWidth = 6f;
        scrollbar.Margin = new Thickness(0, 0, 2 , 0);

        ButtonCreate.OnPressed += OnCreate;
    }

    public void UpdateUI(NewsArticle[] articles, bool publishEnabled, TimeSpan nextPublish, string draftTitle, string draftContent)
    {
        ArticlesContainer.Children.Clear();
        ArticleCount.Text = Loc.GetString("news-write-ui-article-count-text", ("count", articles.Length));

        //Iterate backwards to have the newest article at the top
        for (var i = articles.Length - 1; i >= 0 ; i--)
        {
            var article = articles[i];
            var control = new NewsArticleCard
            {
                Title = article.Title,
                Author = article.Author ?? Loc.GetString("news-read-ui-no-author"),
                PublicationTime = article.ShareTime,
                ArtcileNumber = i
            };
            control.OnDeletePressed += () => DeleteButtonPressed?.Invoke(control.ArtcileNumber);

            ArticlesContainer.AddChild(control);
        }

        ButtonCreate.Disabled = !publishEnabled;
        _nextPublish = nextPublish;

        ArticleEditorPanel.TitleField.Text = draftTitle;
        ArticleEditorPanel.ContentField.TextRope = new Rope.Leaf(draftContent);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        if (!_nextPublish.HasValue)
            return;

        var remainingTime = _nextPublish.Value.Subtract(_gameTiming.CurTime);
        if (remainingTime.TotalSeconds <= 0)
        {
            _nextPublish = null;
            ButtonCreate.Text = Loc.GetString("news-write-ui-create-text");
            return;
        }

        ButtonCreate.Text = remainingTime.Seconds.ToString("D2");
    }

    protected override void Resized()
    {
        base.Resized();
        var margin = ArticleEditorPanel.Margin;
        // Bandaid for the funny 1 pixel margin differences
        ArticleEditorPanel.Margin =  new Thickness(Width - 1, margin.Top, margin.Right, margin.Bottom);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        ButtonCreate.OnPressed -= OnCreate;
    }

    private void OnCreate(BaseButton.ButtonEventArgs buttonEventArgs)
    {
        ArticleEditorPanel.Visible = true;
        CreateButtonPressed?.Invoke();
    }
}
