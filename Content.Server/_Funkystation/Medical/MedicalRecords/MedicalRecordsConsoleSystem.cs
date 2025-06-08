using Content.Shared._Funkystation.Medical.MedicalRecords;
using Content.Shared.Power;

namespace Content.Server._Funkystation.Medical.MedicalRecords;

public sealed class MedicalRecordsConsoleSystem : SharedMedicalRecordsSystem
{
    // handle things like OnGetEntries, OnEmag, OnMapInit, OnPowerChangedEvent, etc...
    // basically any events

    public override void Initialize()
    {
        base.Initialize();

        //SubscribeLocalEvent<MedicalRecordsComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
    }

    //public void OnMapInit(EntityUid uid, MedicalRecordsComponent component, MapInitEvent args) {}
}
