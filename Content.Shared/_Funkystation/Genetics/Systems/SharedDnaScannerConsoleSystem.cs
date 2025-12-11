using System.Linq;
using Content.Shared._Funkystation.Genetics.Components;
using Content.Shared._Funkystation.Genetics.Events;

namespace Content.Shared._Funkystation.Genetics.Systems;

public sealed partial class SharedDnaScannerConsoleSystem : EntitySystem
{
    public void SetSubject(EntityUid consoleUid, EntityUid? subjectUid, DnaScannerConsoleComponent? comp = null)
    {
        if (!Resolve(consoleUid, ref comp))
            return;

        if (comp.CurrentSubject == subjectUid)
            return;

        comp.CurrentSubject = subjectUid;
        RaiseLocalEvent(consoleUid, new DnaScannerSubjectChangedEvent(consoleUid, subjectUid));
    }

    public void ClearSubject(EntityUid consoleUid, DnaScannerConsoleComponent? comp = null)
    {
        if (!Resolve(consoleUid, ref comp))
            return;
        if (comp.CurrentSubject == null)
            return;

        comp.CurrentSubject = null;
        RaiseLocalEvent(consoleUid, new DnaScannerSubjectChangedEvent(consoleUid, null));
    }
}
