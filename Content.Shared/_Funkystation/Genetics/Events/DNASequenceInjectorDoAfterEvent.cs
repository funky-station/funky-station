using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Genetics;

/// <summary>
/// DoAfter event for DNA injectors
/// </summary>
[Serializable, NetSerializable]
public sealed partial class DNASequenceInjectorDoAfterEvent : SimpleDoAfterEvent { }
