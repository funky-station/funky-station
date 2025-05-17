using Content.Shared._Funkystation.Printer.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.Printer;

public partial class SharedPrintingDeviceSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    
    public override void Initialize()
    {
        base.Initialize();
        
    }
}