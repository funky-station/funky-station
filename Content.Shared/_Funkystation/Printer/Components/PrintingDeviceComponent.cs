using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Printer.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PrintingDeviceComponent : Component
{
    [ViewVariables]
    [AutoNetworkedField]
    public List<DocumentTemplatePrototype> AvailableTemplates = new();
}


[Serializable, NetSerializable]
public enum PrintingDeviceUiKey
{
    Key
}

/// <summary>
/// Sent when player wants to print something
/// </summary>
[Serializable, NetSerializable]
public sealed class PrintingDevicePrintRequestMessage(Dictionary<string, string> data, ProtoId<DocumentTemplatePrototype> template) : BoundUserInterfaceMessage
{
    public readonly ProtoId<DocumentTemplatePrototype> Template = template;
    public readonly Dictionary<string, string> Data = data;
}