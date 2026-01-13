using Content.Shared._Funkystation.Paper;
using Content.Shared.Paper;
using static Content.Shared.Paper.PaperComponent;

namespace Content.Server._Funkystation.Paper;

public sealed class BookPaginationSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BookPaginationComponent, BookPageChangeMessage>(OnPageChange);
    }

    private void OnPageChange(EntityUid uid, BookPaginationComponent component, BookPageChangeMessage args)
    {
        if (args.NewPage < 0)
            return;

        if (!TryComp<PaperComponent>(uid, out var paper))
            return;

        var totalPages = CalculateTotalPages(paper.Content, component.LinesPerPage);

        if (args.NewPage >= totalPages)
            return;

        component.CurrentPage = args.NewPage;
        Dirty(uid, component);
    }

    public int CalculateTotalPages(string content, int linesPerPage)
    {
        if (string.IsNullOrEmpty(content))
            return 1;

        var lineCount = content.Split('\n').Length;
        return Math.Max(1, (int) Math.Ceiling((double) lineCount / linesPerPage));
    }
}
