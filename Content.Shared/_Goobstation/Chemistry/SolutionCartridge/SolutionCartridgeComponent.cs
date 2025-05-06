using Content.Shared.Chemistry.Components;

<<<<<<<< HEAD:Content.Shared/_Goobstation/Chemistry/SolutionCartridge/SolutionCartridgeComponent.cs
namespace Content.Shared._Goobstation.Chemistry.SolutionCartridge;
========
namespace Content.Goobstation.Shared.Chemistry.Hypospray;
>>>>>>>> 66ed2ed5c8 (Cartridge autoinjectors overhaul (#2298)):Content.Goobstation.Shared/Chemistry/Hypospray/SolutionCartridgeComponent.cs

[RegisterComponent]
public sealed partial class SolutionCartridgeComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string TargetSolution = "default";

    [DataField(required: true)]
    public Solution Solution;
}
