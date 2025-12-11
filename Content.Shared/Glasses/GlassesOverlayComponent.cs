using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using System;

namespace Content.Shared.Glasses;

[RegisterComponent, NetworkedComponent]
public sealed partial class GlassesOverlayComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField(required: true)]
    public string Shader = string.Empty;

    [DataField(required: true)]
    public Color Color;
}

[Serializable, NetSerializable]
public sealed class GlassesOverlayComponentState : ComponentState
{
    public bool Enabled;
    public string Shader;
    public Color Color;

    public GlassesOverlayComponentState(bool enabled, string shader, Color color)
    {
        Enabled = enabled;
        Shader = shader;
        Color = color;
    }
}
