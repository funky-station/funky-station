using Content.Shared.Atmos;
using Content.Shared.Containers.ItemSlots;
using Content.Server._Funkystation.Atmos.EntitySystems;
using System.Linq;

namespace Content.Server._Funkystation.Atmos.Components
{
    [RegisterComponent]
    public sealed partial class HFRWasteOutputComponent : Component
    {
        [DataField("coreUid")]
        public EntityUid? CoreUid { get; set; }

        [DataField("fusionStarted")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool FusionStarted;

        [DataField("isActive")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsActive;

        [DataField("cracked")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Cracked;
    }
}