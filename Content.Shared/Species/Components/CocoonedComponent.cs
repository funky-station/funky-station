using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Species.Arachnid;

[RegisterComponent, NetworkedComponent]
public sealed partial class CocoonedComponent : Component
{
    /// <summary>
    /// Sprite to display when the entity is cocooned.
    /// TODO: Replace this placeholder with your actual cocoon sprite RSI path and state.
    /// Example: new SpriteSpecifier.Rsi(new("/Textures/Path/To/Cocoon.rsi"), "cocoon_state")
    /// </summary>
    [DataField]
    public SpriteSpecifier Sprite = new SpriteSpecifier.Rsi(new("/Textures/_DV/CosmicCult/Effects/ascendantaura.rsi"), "vfx");
}

[Serializable, NetSerializable]
public enum CocoonedKey
{
    Key
}
