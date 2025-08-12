using System.Threading;
using Robust.Shared.GameStates;

namespace Content.Shared._EE.Carrying;

[RegisterComponent, NetworkedComponent, Access(typeof(CarryingSystem))]
public sealed partial class CarriableComponent : Component
{
    /// <summary>
    /// Number of free hands required
    /// to carry the entity
    /// </summary>
    [DataField]
    public int FreeHandsRequired = 2;

    // begin Frontier edits
    public CancellationTokenSource? CancelToken;

    /// <summary>
    ///     The base duration (In Seconds) of how long it should take to pick up this entity
    ///     before Contests are considered.
    /// </summary>
    [DataField]
    public float PickupDuration = 3;

    // min/max sanitization
    /// <summary>
    ///     The minimum duration (in seconds) of how long it should take to pick up this entity.
    ///     When the strongest, heaviest entity picks this up, it should roughly take this long.
    /// </summary>
    [DataField]
    public float MinPickupDuration = 1.5f;

    /// <summary>
    ///     The maximum duration (in seconds) of how long it should take to pick up this entity.
    ///     When an object picks up the heaviest object it can lift, it should be at most this.
    /// </summary>
    [DataField]
    public float MaxPickupDuration = 6.0f;

    // End Frontier, start Imp

    /// <summary>
    /// Multiplier determining how far this entity has the potential to be thrown, at maximum.
    /// Does not override CCVar clamps.
    /// </summary>
    [ViewVariables]
    public float ThrowPotential = 2f;

    /// <summary>
    /// Multiplier determining how much this entity's mass effects the duration of pickups, at maximum.
    /// Does not override CCVar clamps.
    /// </summary>
    [ViewVariables]
    public float PickupContestPotential = 4f;

    /// <summary>
    /// Multiplier of pickup length when this entity is knocked down.
    /// </summary>
    [ViewVariables]
    public float PickupKnockdownMultiplier = 0.5f;

    /// <summary>
    /// Coefficient applied to a carrying entity's mass to determine whether this entity exceeds the mass threshold for carrying.
    /// eg. weightthreshold of 2 means that this entity can be carried by anything half its mass.
    /// </summary>
    [ViewVariables]
    public float WeightThreshold = 2f;

    /// <summary>
    /// Minimum slowdown multiplier applied to carrier entity.
    /// </summary>
    [ViewVariables]
    public float MinCarrySlowdown = 0.1f;

    /// <summary>
    /// Slowdown multiplier applied to carrier entity before contests.
    /// </summary>
    [ViewVariables]
    public float CarrySlowdown = 0.15f;
}
