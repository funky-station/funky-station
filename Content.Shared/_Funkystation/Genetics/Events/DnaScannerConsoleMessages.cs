using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Genetics.Events;

[Serializable, NetSerializable]
public sealed class DnaScannerSequencerButtonPressedMessage : BoundUserInterfaceMessage
{
    public int ButtonIndex { get; }
    public char NewBase { get; }
    public string MutationId { get; }

    public DnaScannerSequencerButtonPressedMessage(int buttonIndex, char newBase, string mutationId)
    {
        ButtonIndex = buttonIndex;
        NewBase = newBase;
        MutationId = mutationId;
    }
}

[Serializable, NetSerializable]
public sealed class DnaScannerSaveMutationToStorageMessage : BoundUserInterfaceMessage
{
    public readonly string MutationId;

    public DnaScannerSaveMutationToStorageMessage(string mutationId)
    {
        MutationId = mutationId;
    }
}

[Serializable, NetSerializable]
public sealed class DnaScannerDeleteMutationFromStorageMessage : BoundUserInterfaceMessage
{
    public readonly string MutationId;

    public DnaScannerDeleteMutationFromStorageMessage(string mutationId)
    {
        MutationId = mutationId;
    }
}

[Serializable, NetSerializable]
public sealed class DnaScannerPrintActivatorMessage : BoundUserInterfaceMessage
{
    public readonly string MutationId;

    public DnaScannerPrintActivatorMessage(string mutationId)
    {
        MutationId = mutationId;
    }
}

[Serializable, NetSerializable]
public sealed class DnaScannerPrintMutatorMessage : BoundUserInterfaceMessage
{
    public readonly string MutationId;

    public DnaScannerPrintMutatorMessage(string mutationId)
    {
        MutationId = mutationId;
    }
}

[Serializable, NetSerializable]
public sealed class DnaScannerScrambleDnaMessage : BoundUserInterfaceMessage
{
    public DnaScannerScrambleDnaMessage()
    {
    }
}

[Serializable, NetSerializable]
public sealed class DnaScannerDiscoveredMutationsUpdatedMessage : BoundUserInterfaceMessage
{
    public HashSet<string> DiscoveredMutationIds { get; }

    public DnaScannerDiscoveredMutationsUpdatedMessage(HashSet<string> discoveredMutationIds)
    {
        DiscoveredMutationIds = discoveredMutationIds;
    }
}

[Serializable, NetSerializable]
public sealed class DnaScannerToggleResearchMessage : BoundUserInterfaceMessage
{
    public readonly string MutationId;

    public DnaScannerToggleResearchMessage(string mutationId)
    {
        MutationId = mutationId;
    }
}

[Serializable, NetSerializable]
public sealed class DnaScannerUseJokerMessage : BoundUserInterfaceMessage
{
    public DnaScannerUseJokerMessage()
    {
    }
}
