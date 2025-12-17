using Robust.Shared.GameStates;

namespace Content.Server.Medical.Components;

[RegisterComponent]
public sealed partial class BedInternalsComponent : Component
{
    [DataField("slot", required: true)]
    public string GasSlot = default!;

    public bool Enabled;
    public EntityUid? CachedTank;

    [DataField("maskPrototype")]
    public string MaskPrototype = "ClothingMaskBreathMedical";

    public Dictionary<EntityUid, EntityUid> TempMasks = new();

    public Dictionary<EntityUid, EntityUid> StoredMasks = new();
}
