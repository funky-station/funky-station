using Robust.Shared.Serialization;
using Robust.Shared.GameStates;
using Content.Shared._Funkystation.Genetics.Systems;

namespace Content.Shared._Funkystation.Genetics.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedGeneticAnalyzerSystem))]
public sealed partial class GeneticAnalyzerComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? PatientName;

    [DataField, AutoNetworkedField]
    public int PatientInstability;

    [DataField, AutoNetworkedField]
    public List<MutationEntry> Mutations = new();

    [DataField, AutoNetworkedField]
    public HashSet<string> DiscoveredIds = new();
}


[Serializable, NetSerializable]
public enum GeneticAnalyzerUiKey : byte
{
    Key
}

[NetSerializable, Serializable]
public sealed class GeneticAnalyzerUiState : BoundUserInterfaceState
{
    public string? PatientName { get; }
    public int PatientInstability { get; }
    public List<MutationEntry> Mutations { get; }
    public HashSet<string> DiscoveredIds { get; }

    public GeneticAnalyzerUiState(
        string? patientName,
        int patientInstability,
        List<MutationEntry> mutations,
        HashSet<string> discoveredIds)
    {
        PatientName = patientName;
        PatientInstability = patientInstability;
        Mutations = mutations;
        DiscoveredIds = discoveredIds;
    }
}
