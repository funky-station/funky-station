using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Genetics.Events;

[Serializable, NetSerializable]
public sealed partial class GeneticAnalyzerDoAfterEvent : SimpleDoAfterEvent
{
}
