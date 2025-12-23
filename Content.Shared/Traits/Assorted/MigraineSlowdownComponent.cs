namespace Content.Shared.Traits.Assorted;

[RegisterComponent]
public sealed partial class MigraineSlowdownComponent : Component
{
    [DataField] public float Modifier = 0.7f;
}
