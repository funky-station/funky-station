using Content.Shared._Funkystation.Medical.MedicalRecords;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.Medical.MedicalRecords;

public sealed class MedicalRecordsSystem : SharedMedicalRecordsSystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        // NOT for handling the actual events; those should go into server (which is referenced into shared).
        // this is for making sure the bui refreshes properly
        //SubscribeLocalEvent<MedicalRecordsComponent, <T>>(OnSomething);
    }
}
