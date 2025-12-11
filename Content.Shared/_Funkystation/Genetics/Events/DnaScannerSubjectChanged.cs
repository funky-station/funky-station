namespace Content.Shared._Funkystation.Genetics.Events;
public sealed class DnaScannerSubjectChangedEvent : EntityEventArgs
{
    public EntityUid ConsoleUid { get; }
    public EntityUid? SubjectUid { get; }

    public DnaScannerSubjectChangedEvent(EntityUid consoleUid, EntityUid? subjectUid)
    {
        ConsoleUid = consoleUid;
        SubjectUid = subjectUid;
    }
}
