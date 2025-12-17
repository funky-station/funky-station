using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.Genetics.Mutations.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CrawlSpeedBoostComponent : Component
{
    /// <summary>
    ///     New movespeed after laying down.
    ///     .3 = normal crawl speed, 1 = normal walking speed
    /// </summary>
    [DataField] public float TargetSpeedMult = 0.5f;

    // TODO: implement "worn" logic
}
