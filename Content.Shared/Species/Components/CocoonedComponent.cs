using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Species.Arachnid;

[RegisterComponent, NetworkedComponent]
public sealed partial class CocoonedComponent : Component
{
    [DataField]
    public SpriteSpecifier Sprite = new SpriteSpecifier.Rsi(new("/Textures/_DV/CosmicCult/Effects/ascendantaura.rsi"), "vfx");
}

[Serializable, NetSerializable]
public enum CocoonedKey
{
    Key
}
