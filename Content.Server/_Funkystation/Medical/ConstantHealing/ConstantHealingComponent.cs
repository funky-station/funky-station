using Content.Shared.EntityEffects;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

[RegisterComponent]
public sealed partial class ConstantHealingComponent : Component
{
    /// <summary>
    /// Effects to apply every cycle.
    /// </summary>
    [DataField("effects", required: true)]
    public List<EntityEffect> Effects = default!;
    /// <summary>
    ///     The next time that reagents will be metabolized.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate;

    /// <summary>
    ///     How often to metabolize reagents.
    /// </summary>
    /// <returns></returns>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);
}
